using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using Button = System.Windows.Controls.Button;
using System.Windows.Threading;
using PrayerControllerPro.App.Dialogs;
using PrayerControllerPro.App.Services;
using PrayerControllerPro.App.State;
using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.App;

public partial class MainWindow : Window
{
    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");

    private readonly MainViewState _state = new();
    private readonly SettingsStore _settingsStore;
    private readonly IPrayerTimeProvider _prayerTimeProvider;
    private readonly PrayerScheduleComposer _scheduleComposer = new();
    private readonly SchedulerEngine _scheduler = new();
    private readonly AudioPlaybackService _audioPlaybackService = new();
    private readonly Win32MediaController _mediaController = new();
    private readonly VolumeGuardService _volumeGuardService = new();
    private readonly AutoStartService _autoStartService = new();
    private readonly TrayIconService _trayIconService = new();
    private readonly AppLogService _logService;
    private readonly NotificationService _notificationService;
    private readonly AudioPresetDownloadService _audioPresetDownloadService;
    private readonly UpdateCheckService _updateCheckService;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

    private AppSettings _settings = AppCatalog.CreateDefaultSettings();
    private CityDefinition _city = AppCatalog.GetCity("riyadh");
    private DistrictDefinition? _district;
    private DailyPrayerSchedule? _currentSchedule;
    private PrayerExecutionState _executionState = new();
    private bool _isExitRequested;

    public MainWindow()
    {
        InitializeComponent();

        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PrayerControllerPro");
        var settingsPath = Path.Combine(appDataDirectory, "settings.json");
        var cacheDirectory = Path.Combine(appDataDirectory, "cache");
        var logDirectory = Path.Combine(appDataDirectory, "logs");
        var audioCacheDirectory = Path.Combine(appDataDirectory, "audio-cache");

        _logService = new AppLogService(logDirectory);
        _notificationService = new NotificationService(_trayIconService, _logService);
        _audioPresetDownloadService = new AudioPresetDownloadService(audioCacheDirectory, _logService);
        _updateCheckService = new UpdateCheckService(_logService, AppIdentity.CurrentVersion);
        _settingsStore = new SettingsStore(settingsPath);
        _prayerTimeProvider = new AlAdhanPrayerTimeProvider(cacheDirectory);
        _logService.Info("App", "Main window created.");

        DataContext = _state;

        Loaded += OnLoaded;
        Closing += OnClosing;
        _timer.Tick += OnTick;

        RefreshButton.Click += async (_, _) => await RefreshScheduleAsync(manualRefresh: true);
        SettingsButton.Click += OnSettingsClicked;
        AddReminderButton.Click += OnAddReminderClicked;
        AboutButton.Click += async (_, _) => await ShowAboutAsync();
        LogsButton.Click += (_, _) => ShowLogs();
        WidgetModeButton.Click += OnWidgetModeClicked;
        CompactDashboardButton.Click += OnWidgetModeClicked;
        ExitButton.Click += (_, _) => ExitApplication();
        CompactExitButton.Click += (_, _) => ExitApplication();
        AddHandler(Button.ClickEvent, new RoutedEventHandler(OnPrayerButtonClicked));
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _logService.Info("App", "Window loaded.");
        _trayIconService.Initialize(ShowFromTray, ExitApplication);
        UpdateWindowMode();
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _settings = await _settingsStore.LoadAsync();
        _city = AppCatalog.GetCity(_settings.SelectedCityId);
        _district = AppCatalog.GetDistrict(_city.Id, _settings.SelectedDistrictId);
        _logService.Info(
            "Settings",
            "Settings loaded.",
            $"City={_settings.SelectedCityId}; District={_settings.SelectedDistrictId ?? "city-center"}; Method={_settings.CalculationMethod}");
        ApplyAutoStart();

        await RefreshScheduleAsync(manualRefresh: false);
        _timer.Start();
        _logService.Info("App", "Scheduler timer started.");
        _ = CheckForUpdatesAsync(showNoUpdateMessage: false);
    }

    private async void OnTick(object? sender, EventArgs e)
    {
        if (_currentSchedule is null)
        {
            return;
        }

        var now = GetCityNow();
        if (DateOnly.FromDateTime(now.Date) != _currentSchedule.Date)
        {
            await RefreshScheduleAsync(manualRefresh: false);
            return;
        }

        UpdateClock(now);
        UpdateCountdownAndCards(now);
        ExecuteSchedulerActions(now);
        MaintainVolumeGuard(now);
    }

    private async Task RefreshScheduleAsync(bool manualRefresh)
    {
        _state.IsBusy = true;
        _state.StatusText = manualRefresh ? "Refreshing prayer schedule..." : "Loading prayer schedule...";
        _logService.Info("Schedule", manualRefresh ? "Manual schedule refresh started." : "Schedule refresh started.");

        try
        {
            _city = AppCatalog.GetCity(_settings.SelectedCityId);
            _district = AppCatalog.GetDistrict(_city.Id, _settings.SelectedDistrictId);
            var locationText = BuildLocationText(_city, _district);
            var builtInSchedule = await _prayerTimeProvider.GetBuiltInScheduleAsync(_city, _district, _settings.CalculationMethod);
            _currentSchedule = _scheduleComposer.Compose(builtInSchedule, _settings, _city);
            EnsureExecutionStateForSchedule(_currentSchedule);

            _state.CityText = locationText;
            _state.MethodText = AppCatalog.CalculationMethods.FirstOrDefault(method => method.Id == _settings.CalculationMethod)?.DisplayName
                ?? $"Method {_settings.CalculationMethod}";
            _state.SourceText = _currentSchedule.Source;
            _state.StatusText = "Waiting for the next prayer.";

            RebuildPrayerCards(_currentSchedule.Entries, _state.Prayers);
            UpdateClock(GetCityNow());
            UpdateCountdownAndCards(GetCityNow());
            _logService.Info(
                "Schedule",
                "Schedule refreshed.",
                $"Source={_currentSchedule.Source}; Location={_state.CityText}; Entries={_currentSchedule.Entries.Count}");

            if (manualRefresh)
            {
                _ = _notificationService.NotifyAsync(
                    _settings,
                    "Schedule refreshed",
                    $"Loaded {_currentSchedule.Entries.Count} entries for {locationText}.",
                    NotificationEventKind.Schedule);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("Schedule", "Schedule refresh failed.", ex);
            _state.StatusText = "Could not refresh the schedule. The last known data will stay in place.";
            if (manualRefresh)
            {
                System.Windows.MessageBox.Show(this, ex.Message, "Refresh failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        finally
        {
            _state.IsBusy = false;
        }
    }

    private void UpdateClock(DateTimeOffset now)
    {
        _state.CurrentTimeText = now.ToString("hh:mm:ss tt", EnglishCulture);
        _state.CurrentDateText = now.ToString("ddd, dd MMM yyyy", EnglishCulture);
    }

    private void UpdateCountdownAndCards(DateTimeOffset now)
    {
        if (_currentSchedule is null || _currentSchedule.Entries.Count == 0)
        {
            return;
        }

        var countdown = _scheduler.GetCountdown(now, _currentSchedule);
        _state.CountdownPrayerName = countdown.PrayerName;
        _state.CountdownLabel = countdown.Mode == CountdownMode.ResumeAfterPrayer ? "Resume after prayer" : "Next prayer";
        _state.CountdownValue = $"{Math.Max(0, (int)countdown.Remaining.TotalHours):00}:{countdown.Remaining.Minutes:00}:{countdown.Remaining.Seconds:00}";

        var countdownEntry = _currentSchedule.Entries.FirstOrDefault(entry => entry.Id == countdown.PrayerId);
        if (countdown.Mode == CountdownMode.ResumeAfterPrayer)
        {
            _state.CountdownTargetText = $"Resume at {countdown.TargetTime.ToString("hh:mm tt", EnglishCulture)}";
            _state.CountdownTargetTimeText = countdown.TargetTime.ToString("hh:mm tt", EnglishCulture);
            _state.CountdownMetaText = countdownEntry is null
                ? "Automation active"
                : $"Paused for {countdownEntry.DisplayName}";
        }
        else
        {
            _state.CountdownTargetText = countdownEntry is null
                ? "Adhan at --"
                : $"Adhan at {countdownEntry.PrayerTime.ToString("hh:mm tt", EnglishCulture)}";
            _state.CountdownTargetTimeText = countdownEntry is null
                ? "--"
                : countdownEntry.PrayerTime.ToString("hh:mm tt", EnglishCulture);
            _state.CountdownMetaText = countdownEntry is null
                ? "Iqama at --"
                : $"Iqama at {countdownEntry.IqamaTime.ToString("hh:mm tt", EnglishCulture)}";
        }

        foreach (var card in _state.Prayers)
        {
            card.IsActive = countdown.Mode == CountdownMode.ResumeAfterPrayer && card.Id == countdown.PrayerId;
            card.IsNext = countdown.Mode == CountdownMode.NextPrayer && card.Id == countdown.PrayerId;
        }

        _trayIconService.UpdateTooltip(BuildLocationName(_city, _district), countdown.PrayerName, _state.CountdownValue);
    }

    private void ExecuteSchedulerActions(DateTimeOffset now)
    {
        if (_currentSchedule is null)
        {
            return;
        }

        foreach (var action in _scheduler.Evaluate(now, _currentSchedule, _executionState))
        {
            switch (action.Kind)
            {
                case SchedulerActionKind.PauseMedia:
                    if (_settings.Audio.MediaControlMode == MediaControlMode.VolumeGuard)
                    {
                        var protectedCount = _volumeGuardService.Protect(
                            _settings.Audio.VolumeGuardLevel,
                            _settings.Audio.VolumeGuardTransitionMode);
                        var message = $"Media volume lowered for {action.PrayerName}. Protected sessions: {protectedCount}.";
                        _state.StatusText = $"Media volume protected for {action.PrayerName}.";
                        _logService.Info("Scheduler", $"Media volume protected for {action.PrayerName}.", message);
                        _ = _notificationService.NotifyAsync(_settings, "Media volume protected", message, NotificationEventKind.Media);
                        break;
                    }

                    _mediaController.TogglePlayPause();
                    _state.StatusText = $"Media paused for {action.PrayerName}.";
                    _logService.Info("Scheduler", $"Media paused for {action.PrayerName}.", action.Message);
                    _ = _notificationService.NotifyAsync(_settings, "Media paused", action.Message, NotificationEventKind.Media);
                    break;

                case SchedulerActionKind.ResumeMedia:
                    if (_settings.Audio.MediaControlMode == MediaControlMode.VolumeGuard)
                    {
                        var restoredCount = _volumeGuardService.Restore(_settings.Audio.VolumeGuardTransitionMode);
                        _logService.Info("Scheduler", $"Media volume restored after {action.PrayerName}.", $"Restored sessions: {restoredCount}.");
                        _ = _notificationService.NotifyAsync(_settings, "Media volume restored", $"Media volume restored after {action.PrayerName}.", NotificationEventKind.Media);
                    }
                    else
                    {
                        _mediaController.TogglePlayPause();
                        _logService.Info("Scheduler", $"Media resumed after {action.PrayerName}.", action.Message);
                        _ = _notificationService.NotifyAsync(_settings, "Media resumed", action.Message, NotificationEventKind.Media);
                    }

                    _state.StatusText = "Waiting for the next prayer.";
                    break;

                case SchedulerActionKind.PlayAdhan:
                    if (_settings.Audio.EnableAdhanAudio)
                    {
                        _audioPlaybackService.Play(_settings.Audio.AdhanAudioPath, _settings.Audio.Volume);
                        _logService.Info("Audio", $"Adhan audio requested for {action.PrayerName}.", _settings.Audio.AdhanAudioPath);
                    }

                    _state.StatusText = $"Adhan notification for {action.PrayerName}.";
                    _logService.Info("Scheduler", $"Adhan event for {action.PrayerName}.", action.Message);
                    _ = _notificationService.NotifyAsync(_settings, "Adhan", action.Message, NotificationEventKind.Audio);
                    break;

                case SchedulerActionKind.PlayIqama:
                    if (_settings.Audio.EnableIqamaAudio)
                    {
                        _audioPlaybackService.Play(_settings.Audio.IqamaAudioPath, _settings.Audio.Volume);
                        _logService.Info("Audio", $"Iqama audio requested for {action.PrayerName}.", _settings.Audio.IqamaAudioPath);
                    }

                    _state.StatusText = $"Iqama notification for {action.PrayerName}.";
                    _logService.Info("Scheduler", $"Iqama event for {action.PrayerName}.", action.Message);
                    _ = _notificationService.NotifyAsync(_settings, "Iqama", action.Message, NotificationEventKind.Audio);
                    break;
            }
        }
    }

    private async void OnSettingsClicked(object? sender, RoutedEventArgs e)
    {
        _logService.Info("Settings", "Settings window opened.");
        var dialog = new AppSettingsWindow(_settings, _audioPlaybackService, _notificationService, _audioPresetDownloadService) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
        {
            _logService.Info("Settings", "Settings window closed without saving.");
            return;
        }

        _settings = dialog.Result;
        AppCatalog.EnsureDefaults(_settings);
        await _settingsStore.SaveAsync(_settings);
        if (_settings.Audio.MediaControlMode != MediaControlMode.VolumeGuard)
        {
            _volumeGuardService.Restore(_settings.Audio.VolumeGuardTransitionMode);
        }

        _logService.Info(
            "Settings",
            "Settings saved.",
            $"City={_settings.SelectedCityId}; District={_settings.SelectedDistrictId ?? "city-center"}; Method={_settings.CalculationMethod}");
        ApplyAutoStart();
        await RefreshScheduleAsync(manualRefresh: false);
    }

    private async void OnAddReminderClicked(object? sender, RoutedEventArgs e)
    {
        var dialog = new ReminderWindow { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
        {
            return;
        }

        _settings.CustomReminders.Add(dialog.Result);
        _settings.PrayerRules[dialog.Result.Id] = dialog.ResultRule ?? AppCatalog.CreateDefaultRule(isCustom: true);
        await _settingsStore.SaveAsync(_settings);
        _logService.Info("Reminders", "Custom reminder added.", $"{dialog.Result.Name} at {dialog.Result.Time:HH\\:mm}");
        await RefreshScheduleAsync(manualRefresh: false);
    }

    private void OnWidgetModeClicked(object? sender, RoutedEventArgs e)
    {
        _state.IsWidgetMode = !_state.IsWidgetMode;
        UpdateWindowMode();
        _logService.Info("UI", _state.IsWidgetMode ? "Switched to widget mode." : "Switched to dashboard mode.");
    }

    private async Task ShowAboutAsync()
    {
        _logService.Info("App", "About dialog opened.");
        var dialog = new AboutWindow(_settings.Updates.LastUpdateCheckUtc) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            await CheckForUpdatesAsync(showNoUpdateMessage: true);
        }
    }

    private async Task CheckForUpdatesAsync(bool showNoUpdateMessage)
    {
        if (!showNoUpdateMessage && !_settings.Updates.CheckForUpdatesAutomatically)
        {
            return;
        }

        var result = await _updateCheckService.CheckAsync();
        _settings.Updates.LastUpdateCheckUtc = DateTimeOffset.UtcNow;
        await _settingsStore.SaveAsync(_settings);

        if (!result.IsSuccess)
        {
            if (showNoUpdateMessage)
            {
                System.Windows.MessageBox.Show(
                    this,
                    result.ErrorMessage ?? "Could not check for updates.",
                    "Update check failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            return;
        }

        if (!result.HasUpdate || result.Manifest is null)
        {
            if (showNoUpdateMessage)
            {
                System.Windows.MessageBox.Show(
                    this,
                    "You are running the latest version.",
                    "Updates",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            return;
        }

        if (!showNoUpdateMessage
            && !result.Manifest.Mandatory
            && string.Equals(_settings.Updates.SkippedUpdateVersion, result.Manifest.LatestVersion, StringComparison.OrdinalIgnoreCase))
        {
            _logService.Info("Updates", "Available update was skipped by user.", result.Manifest.LatestVersion);
            return;
        }

        _ = _notificationService.NotifyAsync(
            _settings,
            string.IsNullOrWhiteSpace(result.Manifest.Title) ? "Update available" : result.Manifest.Title,
            $"Version {result.Manifest.LatestVersion} is available.",
            NotificationEventKind.App);

        var dialog = new UpdateAvailableWindow(result.Manifest, result.CurrentVersion) { Owner = this };
        dialog.ShowDialog();

        if (dialog.SkipVersion)
        {
            _settings.Updates.SkippedUpdateVersion = result.Manifest.LatestVersion;
            await _settingsStore.SaveAsync(_settings);
            _logService.Info("Updates", "User skipped update version.", result.Manifest.LatestVersion);
        }
    }

    private void ShowLogs()
    {
        _logService.Info("Logs", "Logs window opened.");
        var dialog = new LogsWindow(_logService) { Owner = this };
        dialog.ShowDialog();
    }

    private async void OnPrayerButtonClicked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not Button button || button.Tag is not string prayerId)
        {
            return;
        }

        if (_currentSchedule is null)
        {
            return;
        }

        if (button.Name == "DeleteReminderButton")
        {
            await DeleteReminderAsync(prayerId);
            return;
        }

        if (button.Name != "EditPrayerButton")
        {
            return;
        }

        var entry = _currentSchedule.Entries.FirstOrDefault(item => item.Id == prayerId);
        if (entry is null)
        {
            return;
        }

        if (entry.IsCustom)
        {
            await EditReminderAsync(entry.Id);
            return;
        }

        var defaultIqamaOffset = entry.IsCustom ? 0 : AppCatalog.GetBuiltInPrayer(entry.Id).DefaultIqamaOffsetMinutes;
        var dialog = new PrayerRuleWindow(entry.DisplayName, entry.Rule, defaultIqamaOffset) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
        {
            return;
        }

        _settings.PrayerRules[entry.Id] = dialog.Result;
        await _settingsStore.SaveAsync(_settings);
        _logService.Info("Automation", $"Prayer automation updated for {entry.DisplayName}.");
        await RefreshScheduleAsync(manualRefresh: false);
    }

    private async Task EditReminderAsync(string reminderId)
    {
        var reminder = _settings.CustomReminders.FirstOrDefault(item => item.Id == reminderId);
        if (reminder is null)
        {
            return;
        }

        var currentRule = _settings.PrayerRules.TryGetValue(reminder.Id, out var storedRule)
            ? storedRule
            : AppCatalog.CreateDefaultRule(isCustom: true);
        var dialog = new ReminderWindow(reminder, currentRule) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
        {
            return;
        }

        reminder.Name = dialog.Result.Name;
        reminder.Time = dialog.Result.Time;
        _settings.PrayerRules[reminder.Id] = dialog.ResultRule ?? currentRule;
        _executionState.Markers.Remove(reminder.Id);

        await _settingsStore.SaveAsync(_settings);
        _logService.Info("Reminders", "Custom reminder edited.", $"{reminder.Name} at {reminder.Time:HH\\:mm}");
        await RefreshScheduleAsync(manualRefresh: false);
    }

    private async Task DeleteReminderAsync(string reminderId)
    {
        var reminder = _settings.CustomReminders.FirstOrDefault(item => item.Id == reminderId);
        if (reminder is null)
        {
            return;
        }

        var confirmation = System.Windows.MessageBox.Show(
            this,
            $"Delete the reminder \"{reminder.Name}\"?",
            "Delete reminder",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        _settings.CustomReminders.Remove(reminder);
        _settings.PrayerRules.Remove(reminder.Id);
        await _settingsStore.SaveAsync(_settings);
        _logService.Info("Reminders", "Custom reminder deleted.", reminder.Name);
        await RefreshScheduleAsync(manualRefresh: false);
    }

    private void ShowFromTray()
    {
        _logService.Info("App", "Window restored from tray.");
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _logService.Info("App", "Application exit requested.");
        _isExitRequested = true;
        _trayIconService.Dispose();
        _volumeGuardService.Restore(_settings.Audio.VolumeGuardTransitionMode);
        _audioPlaybackService.Dispose();
        _notificationService.Dispose();
        _audioPresetDownloadService.Dispose();
        _updateCheckService.Dispose();
        Close();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_isExitRequested)
        {
            return;
        }

        e.Cancel = true;
        Hide();
        _logService.Info("App", "Window hidden to system tray.");
        _ = _notificationService.NotifyAsync(
            _settings,
            AppIdentity.ProductName,
            "The app is still running in the system tray.",
            NotificationEventKind.App,
            force: true);
    }

    private void ApplyAutoStart()
    {
        _autoStartService.Apply(_settings.AutoStart, Environment.ProcessPath ?? string.Empty);
        _logService.Info("Settings", _settings.AutoStart ? "Windows auto-start enabled." : "Windows auto-start disabled.");
    }

    private DateTimeOffset GetCityNow()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_city.TimeZoneId);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone);
    }

    private static string BuildLocationText(CityDefinition city, DistrictDefinition? district)
    {
        return district is null
            ? $"{city.DisplayName}, {city.ApiCountry}"
            : $"{city.DisplayName} - {district.DisplayName}, {city.ApiCountry}";
    }

    private static string BuildLocationName(CityDefinition city, DistrictDefinition? district)
    {
        return district is null
            ? city.DisplayName
            : $"{city.DisplayName} - {district.DisplayName}";
    }

    private void EnsureExecutionStateForSchedule(DailyPrayerSchedule schedule)
    {
        if (_executionState.Date != schedule.Date)
        {
            _executionState.Reset(schedule.Date);
        }
    }

    private void MaintainVolumeGuard(DateTimeOffset now)
    {
        if (_currentSchedule is null || _settings.Audio.MediaControlMode != MediaControlMode.VolumeGuard)
        {
            if (_volumeGuardService.IsActive)
            {
                _volumeGuardService.Restore(_settings.Audio.VolumeGuardTransitionMode);
            }

            return;
        }

        var countdown = _scheduler.GetCountdown(now, _currentSchedule);
        if (countdown.Mode == CountdownMode.ResumeAfterPrayer)
        {
            _volumeGuardService.Protect(
                _settings.Audio.VolumeGuardLevel,
                _settings.Audio.VolumeGuardTransitionMode);
            return;
        }

        if (_volumeGuardService.IsActive)
        {
            _volumeGuardService.Restore(_settings.Audio.VolumeGuardTransitionMode);
        }
    }

    private void UpdateWindowMode()
    {
        if (_state.IsWidgetMode)
        {
            Width = 340;
            Height = 158;
            MinWidth = 340;
            MinHeight = 158;
            Topmost = true;
            Title = "Prayer Controller Pro Widget";
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = true;
            return;
        }

        Width = 460;
        Height = 860;
        MinWidth = 460;
        MinHeight = 800;
        Topmost = false;
        Title = AppIdentity.ProductName;
        ResizeMode = ResizeMode.CanMinimize;
        WindowStyle = WindowStyle.SingleBorderWindow;
        ShowInTaskbar = true;
    }

    private void WidgetHeader_OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_state.IsWidgetMode && e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private static void RebuildPrayerCards(IEnumerable<PrayerScheduleEntry> entries, ObservableCollection<PrayerCardState> target)
    {
        target.Clear();
        foreach (var entry in entries)
        {
            target.Add(new PrayerCardState(entry));
        }
    }
}
