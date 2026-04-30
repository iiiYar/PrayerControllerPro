using Microsoft.Win32;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using PrayerControllerPro.App.Services;
using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.App.Dialogs;

public partial class AppSettingsWindow : Window
{
    private static readonly MediaControlModeOption[] MediaControlModeOptions =
    [
        new(MediaControlMode.PlayPauseKey, "Play/Pause key (current method)"),
        new(MediaControlMode.VolumeGuard, "Volume guard (lower other app volumes)")
    ];

    private static readonly VolumeGuardPresetOption[] VolumeGuardPresetOptions =
    [
        new("0% - Silent", 0d),
        new("3% - Very quiet", 0.03d),
        new("5% - Low", 0.05d),
        new("Custom", null)
    ];

    private static readonly VolumeGuardTransitionModeOption[] VolumeGuardTransitionModeOptions =
    [
        new(VolumeGuardTransitionMode.Fast, "Fast - 2 steps"),
        new(VolumeGuardTransitionMode.Slow, "Slow - 4 steps"),
        new(VolumeGuardTransitionMode.Smooth, "Smooth - 8 steps"),
        new(VolumeGuardTransitionMode.Smoother, "Smoother - 12 steps")
    ];

    private readonly AudioPlaybackService _audioPlaybackService;
    private readonly NotificationService _notificationService;
    private readonly AudioPresetDownloadService _audioPresetDownloadService;
    private readonly AppSettings _sourceSettings;
    private bool _isPreviewPlaybackActive;

    public AppSettingsWindow(
        AppSettings settings,
        AudioPlaybackService audioPlaybackService,
        NotificationService notificationService,
        AudioPresetDownloadService audioPresetDownloadService)
    {
        InitializeComponent();
        _audioPlaybackService = audioPlaybackService;
        _notificationService = notificationService;
        _audioPresetDownloadService = audioPresetDownloadService;
        _sourceSettings = settings;

        CityComboBox.ItemsSource = AppCatalog.SupportedCities;
        MethodComboBox.ItemsSource = AppCatalog.CalculationMethods;
        MediaControlModeComboBox.ItemsSource = MediaControlModeOptions;
        VolumeGuardLevelComboBox.ItemsSource = VolumeGuardPresetOptions;
        VolumeGuardTransitionModeComboBox.ItemsSource = VolumeGuardTransitionModeOptions;

        CityComboBox.SelectedItem = AppCatalog.GetCity(settings.SelectedCityId);
        UpdateDistrictOptions(settings.SelectedDistrictId);
        MethodComboBox.SelectedItem = AppCatalog.CalculationMethods.First(method => method.Id == settings.CalculationMethod);
        AutoStartCheckBox.IsChecked = settings.AutoStart;
        AutoUpdateCheckBox.IsChecked = settings.Updates.CheckForUpdatesAutomatically;
        MediaControlModeComboBox.SelectedItem = MediaControlModeOptions.First(option => option.Mode == settings.Audio.MediaControlMode);
        SelectVolumeGuardPreset(settings.Audio.VolumeGuardLevel);
        VolumeGuardTransitionModeComboBox.SelectedItem = VolumeGuardTransitionModeOptions.First(option => option.Mode == settings.Audio.VolumeGuardTransitionMode);
        CustomVolumeGuardTextBox.Text = (settings.Audio.VolumeGuardLevel * 100d).ToString("0.##", CultureInfo.InvariantCulture);
        EnableAdhanAudioCheckBox.IsChecked = settings.Audio.EnableAdhanAudio;
        EnableIqamaAudioCheckBox.IsChecked = settings.Audio.EnableIqamaAudio;
        AdhanPathTextBox.Text = settings.Audio.AdhanAudioPath ?? string.Empty;
        IqamaPathTextBox.Text = settings.Audio.IqamaAudioPath ?? string.Empty;
        AdhanPresetUrlTextBox.Text = settings.Audio.AdhanPresetUrl ?? string.Empty;
        IqamaPresetUrlTextBox.Text = settings.Audio.IqamaPresetUrl ?? string.Empty;
        AdhanPresetStatusTextBlock.Text = _audioPresetDownloadService.IsCachedPath(settings.Audio.AdhanAudioPath)
            ? "Preset cached locally and ready for offline playback."
            : "No preset downloaded yet.";
        IqamaPresetStatusTextBlock.Text = _audioPresetDownloadService.IsCachedPath(settings.Audio.IqamaAudioPath)
            ? "Preset cached locally and ready for offline playback."
            : "No preset downloaded yet.";
        VolumeSlider.Value = settings.Audio.Volume;
        EnableWindowsNotificationsCheckBox.IsChecked = settings.Notifications.EnableWindowsNotifications;
        EnableDiscordNotificationsCheckBox.IsChecked = settings.Notifications.EnableDiscordNotifications;
        DiscordWebhookTextBox.Text = settings.Notifications.DiscordWebhookUrl ?? string.Empty;
        NotifyScheduleCheckBox.IsChecked = settings.Notifications.NotifyOnScheduleRefresh;
        NotifyMediaCheckBox.IsChecked = settings.Notifications.NotifyOnMediaActions;
        NotifyAudioCheckBox.IsChecked = settings.Notifications.NotifyOnAudioEvents;
        NotifyAppCheckBox.IsChecked = settings.Notifications.NotifyOnAppEvents;

        CityComboBox.SelectionChanged += (_, _) => UpdateDistrictOptions(selectedDistrictId: null);
        MediaControlModeComboBox.SelectionChanged += (_, _) => UpdateVolumeGuardControls();
        VolumeGuardLevelComboBox.SelectionChanged += (_, _) => UpdateVolumeGuardControls();
        BrowseAdhanButton.Click += (_, _) => BrowseForAudio(AdhanPathTextBox);
        BrowseIqamaButton.Click += (_, _) => BrowseForAudio(IqamaPathTextBox);
        DownloadAdhanPresetButton.Click += async (_, _) => await DownloadPresetAsync("Adhan", AdhanPresetUrlTextBox, AdhanPathTextBox, AdhanPresetStatusTextBlock);
        DownloadIqamaPresetButton.Click += async (_, _) => await DownloadPresetAsync("Iqama", IqamaPresetUrlTextBox, IqamaPathTextBox, IqamaPresetStatusTextBlock);
        ClearAdhanPresetButton.Click += (_, _) => ClearCachedPreset("Adhan", AdhanPathTextBox, AdhanPresetStatusTextBlock);
        ClearIqamaPresetButton.Click += (_, _) => ClearCachedPreset("Iqama", IqamaPathTextBox, IqamaPresetStatusTextBlock);
        TestAdhanButton.Click += (_, _) => PlayPreview(AdhanPathTextBox.Text);
        TestIqamaButton.Click += (_, _) => PlayPreview(IqamaPathTextBox.Text);
        StopAdhanButton.Click += (_, _) => StopPreview();
        StopIqamaButton.Click += (_, _) => StopPreview();
        TestDiscordButton.Click += OnTestDiscordClicked;
        SaveButton.Click += OnSaveClicked;
        CancelButton.Click += (_, _) => Close();
        Closed += (_, _) => StopPreview();
        UpdateVolumeGuardControls();
    }

    public AppSettings? Result { get; private set; }

    private void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        var selectedCity = CityComboBox.SelectedItem as CityDefinition ?? AppCatalog.SupportedCities[0];
        var selectedDistrict = (DistrictComboBox.SelectedItem as DistrictOption)?.District;
        var selectedMethod = MethodComboBox.SelectedItem as CalculationMethodDefinition ?? AppCatalog.CalculationMethods[0];
        var selectedMediaMode = (MediaControlModeComboBox.SelectedItem as MediaControlModeOption)?.Mode ?? MediaControlMode.PlayPauseKey;
        var selectedTransitionMode = (VolumeGuardTransitionModeComboBox.SelectedItem as VolumeGuardTransitionModeOption)?.Mode ?? VolumeGuardTransitionMode.Fast;

        if (!TryReadVolumeGuardLevel(selectedMediaMode, out var volumeGuardLevel))
        {
            return;
        }

        Result = new AppSettings
        {
            SelectedCityId = selectedCity.Id,
            SelectedDistrictId = selectedDistrict?.Id,
            CalculationMethod = selectedMethod.Id,
            AutoStart = AutoStartCheckBox.IsChecked == true,
            Theme = _sourceSettings.Theme,
            Updates = new UpdateSettings
            {
                CheckForUpdatesAutomatically = AutoUpdateCheckBox.IsChecked == true,
                LastUpdateCheckUtc = _sourceSettings.Updates.LastUpdateCheckUtc,
                SkippedUpdateVersion = _sourceSettings.Updates.SkippedUpdateVersion
            },
            CustomReminders = _sourceSettings.CustomReminders,
            PrayerRules = _sourceSettings.PrayerRules,
            Audio = new AudioSettings
            {
                EnableAdhanAudio = EnableAdhanAudioCheckBox.IsChecked == true,
                EnableIqamaAudio = EnableIqamaAudioCheckBox.IsChecked == true,
                AdhanAudioPath = string.IsNullOrWhiteSpace(AdhanPathTextBox.Text) ? null : AdhanPathTextBox.Text.Trim(),
                IqamaAudioPath = string.IsNullOrWhiteSpace(IqamaPathTextBox.Text) ? null : IqamaPathTextBox.Text.Trim(),
                AdhanPresetUrl = string.IsNullOrWhiteSpace(AdhanPresetUrlTextBox.Text) ? null : AdhanPresetUrlTextBox.Text.Trim(),
                IqamaPresetUrl = string.IsNullOrWhiteSpace(IqamaPresetUrlTextBox.Text) ? null : IqamaPresetUrlTextBox.Text.Trim(),
                Volume = VolumeSlider.Value,
                MediaControlMode = selectedMediaMode,
                VolumeGuardLevel = volumeGuardLevel,
                VolumeGuardTransitionMode = selectedTransitionMode
            },
            Notifications = BuildNotificationSettingsFromForm()
        };

        DialogResult = true;
        Close();
    }

    private void PlayPreview(string? filePath)
    {
        if (!_audioPlaybackService.CanPlay(filePath))
        {
            System.Windows.MessageBox.Show(this, "Choose an existing audio file first.", "Audio test", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _isPreviewPlaybackActive = true;
        _audioPlaybackService.Play(filePath, VolumeSlider.Value);
    }

    private void StopPreview()
    {
        if (!_isPreviewPlaybackActive)
        {
            return;
        }

        _audioPlaybackService.Stop();
        _isPreviewPlaybackActive = false;
    }

    private async void OnTestDiscordClicked(object? sender, RoutedEventArgs e)
    {
        var settings = BuildNotificationSettingsFromForm();
        if (string.IsNullOrWhiteSpace(settings.DiscordWebhookUrl))
        {
            System.Windows.MessageBox.Show(this, "Add a Discord webhook URL first.", "Discord test", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sent = await _notificationService.SendDiscordTestAsync(settings);
        System.Windows.MessageBox.Show(
            this,
            sent ? "Discord test notification was sent." : "Discord test failed. Check the webhook URL and Logs.",
            "Discord test",
            MessageBoxButton.OK,
            sent ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private NotificationSettings BuildNotificationSettingsFromForm()
    {
        return new NotificationSettings
        {
            EnableWindowsNotifications = EnableWindowsNotificationsCheckBox.IsChecked == true,
            EnableDiscordNotifications = EnableDiscordNotificationsCheckBox.IsChecked == true,
            DiscordWebhookUrl = string.IsNullOrWhiteSpace(DiscordWebhookTextBox.Text) ? null : DiscordWebhookTextBox.Text.Trim(),
            NotifyOnScheduleRefresh = NotifyScheduleCheckBox.IsChecked == true,
            NotifyOnMediaActions = NotifyMediaCheckBox.IsChecked == true,
            NotifyOnAudioEvents = NotifyAudioCheckBox.IsChecked == true,
            NotifyOnAppEvents = NotifyAppCheckBox.IsChecked == true
        };
    }

    private void UpdateDistrictOptions(string? selectedDistrictId)
    {
        var selectedCity = CityComboBox.SelectedItem as CityDefinition ?? AppCatalog.SupportedCities[0];
        var options = new List<DistrictOption> { new("Use city center", null) };
        options.AddRange(AppCatalog.GetDistrictsForCity(selectedCity.Id).Select(district => new DistrictOption(district.DisplayName, district)));

        DistrictComboBox.ItemsSource = options;
        DistrictComboBox.SelectedItem = options.FirstOrDefault(option =>
            option.District is not null
            && string.Equals(option.District.Id, selectedDistrictId, StringComparison.OrdinalIgnoreCase))
            ?? options[0];

        var hasDistricts = options.Count > 1;
        var visibility = hasDistricts ? Visibility.Visible : Visibility.Collapsed;
        DistrictLabel.Visibility = visibility;
        DistrictComboBox.Visibility = visibility;
        DistrictHelpTextBlock.Visibility = visibility;
    }

    private async Task DownloadPresetAsync(
        string kind,
        System.Windows.Controls.TextBox urlTextBox,
        System.Windows.Controls.TextBox targetPathTextBox,
        TextBlock statusTextBlock)
    {
        if (string.IsNullOrWhiteSpace(urlTextBox.Text))
        {
            statusTextBlock.Text = "Add a preset URL first.";
            System.Windows.MessageBox.Show(this, "Add a preset URL first.", $"{kind} preset", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        statusTextBlock.Text = "Downloading...";

        try
        {
            var downloadedPath = await _audioPresetDownloadService.DownloadAsync(urlTextBox.Text.Trim(), kind);
            targetPathTextBox.Text = downloadedPath;
            statusTextBlock.Text = "Downloaded and ready for offline playback.";
        }
        catch (Exception ex)
        {
            statusTextBlock.Text = "Download failed. Check Logs for details.";
            System.Windows.MessageBox.Show(this, ex.Message, $"{kind} preset download failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ClearCachedPreset(string kind, System.Windows.Controls.TextBox targetPathTextBox, TextBlock statusTextBlock)
    {
        var cleared = _audioPresetDownloadService.TryClearCachedFile(targetPathTextBox.Text);
        if (cleared)
        {
            targetPathTextBox.Text = string.Empty;
            statusTextBlock.Text = "Cached preset removed.";
            return;
        }

        statusTextBlock.Text = "No cached preset file to remove.";
    }

    private void UpdateVolumeGuardControls()
    {
        var isVolumeGuard = (MediaControlModeComboBox.SelectedItem as MediaControlModeOption)?.Mode == MediaControlMode.VolumeGuard;
        var isCustom = (VolumeGuardLevelComboBox.SelectedItem as VolumeGuardPresetOption)?.Value is null;

        VolumeGuardSettingsPanel.Visibility = isVolumeGuard ? Visibility.Visible : Visibility.Collapsed;
        CustomVolumeGuardTextBox.IsEnabled = isVolumeGuard && isCustom;
        CustomVolumeGuardTextBox.Opacity = CustomVolumeGuardTextBox.IsEnabled ? 1d : 0.55d;
    }

    private void SelectVolumeGuardPreset(double volumeGuardLevel)
    {
        var preset = VolumeGuardPresetOptions.FirstOrDefault(option => option.Value is not null && Math.Abs(option.Value.Value - volumeGuardLevel) < 0.001d)
            ?? VolumeGuardPresetOptions[^1];
        VolumeGuardLevelComboBox.SelectedItem = preset;
    }

    private bool TryReadVolumeGuardLevel(MediaControlMode selectedMediaMode, out double volumeGuardLevel)
    {
        if ((VolumeGuardLevelComboBox.SelectedItem as VolumeGuardPresetOption)?.Value is double presetValue)
        {
            volumeGuardLevel = presetValue;
            return true;
        }

        if (double.TryParse(CustomVolumeGuardTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var customPercent)
            || double.TryParse(CustomVolumeGuardTextBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out customPercent))
        {
            if (customPercent is >= 0d and <= 100d)
            {
                volumeGuardLevel = customPercent / 100d;
                return true;
            }
        }

        volumeGuardLevel = _sourceSettings.Audio.VolumeGuardLevel;
        if (selectedMediaMode != MediaControlMode.VolumeGuard)
        {
            return true;
        }

        System.Windows.MessageBox.Show(this, "Custom volume guard level must be a number from 0 to 100.", "Volume guard", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    private static void BrowseForAudio(System.Windows.Controls.TextBox target)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Audio Files|*.mp3;*.wav;*.wma;*.aac|All Files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            target.Text = dialog.FileName;
        }
    }

    private sealed record MediaControlModeOption(MediaControlMode Mode, string DisplayName);

    private sealed record VolumeGuardPresetOption(string DisplayName, double? Value);

    private sealed record VolumeGuardTransitionModeOption(VolumeGuardTransitionMode Mode, string DisplayName);

    private sealed record DistrictOption(string DisplayName, DistrictDefinition? District);
}
