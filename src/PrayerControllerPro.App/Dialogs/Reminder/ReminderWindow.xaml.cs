using System.Globalization;
using System.Windows;
using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.App.Dialogs.Reminder;

public partial class ReminderWindow : Window
{
    private static readonly IReadOnlyList<string> HourOptions = Enumerable.Range(1, 12)
        .Select(hour => hour.ToString("00", CultureInfo.InvariantCulture))
        .ToArray();

    private static readonly IReadOnlyList<string> MinuteOptions = Enumerable.Range(0, 60)
        .Select(minute => minute.ToString("00", CultureInfo.InvariantCulture))
        .ToArray();

    private static readonly IReadOnlyList<string> PeriodOptions = ["AM", "PM"];

    private readonly CustomReminder? _existingReminder;
    private readonly PrayerRuleSettings _currentRule;

    public ReminderWindow(CustomReminder? reminder = null, PrayerRuleSettings? rule = null)
    {
        InitializeComponent();
        _existingReminder = reminder;
        _currentRule = (rule ?? AppCatalog.CreateDefaultRule(isCustom: true)).Clone();

        TitleTextBlock.Text = reminder is null ? "Add reminder" : "Edit reminder";
        NameTextBox.Text = reminder?.Name ?? string.Empty;
        HourComboBox.ItemsSource = HourOptions;
        MinuteComboBox.ItemsSource = MinuteOptions;
        PeriodComboBox.ItemsSource = PeriodOptions;
        SetSelectedTime(reminder?.Time ?? new TimeOnly(20, 0));
        PauseBeforeTextBox.Text = Math.Max(0, _currentRule.StopBeforeMinutes).ToString(CultureInfo.InvariantCulture);
        ResumeAfterTextBox.Text = Math.Max(0, _currentRule.ResumeAfterMinutes).ToString(CultureInfo.InvariantCulture);

        SaveButton.Click += OnSaveClicked;
        CancelButton.Click += (_, _) => Close();
    }

    public CustomReminder? Result { get; private set; }

    public PrayerRuleSettings? ResultRule { get; private set; }

    private void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            System.Windows.MessageBox.Show(this, "Reminder name is required.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (HourComboBox.SelectedItem is not string hourText
            || MinuteComboBox.SelectedItem is not string minuteText
            || PeriodComboBox.SelectedItem is not string periodText
            || !int.TryParse(hourText, out var hour)
            || !int.TryParse(minuteText, out var minute))
        {
            System.Windows.MessageBox.Show(this, "Please choose a valid reminder time.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(PauseBeforeTextBox.Text.Trim(), out var pauseBefore))
        {
            System.Windows.MessageBox.Show(this, "Pause before minutes must be a whole number.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(ResumeAfterTextBox.Text.Trim(), out var resumeAfter))
        {
            System.Windows.MessageBox.Show(this, "Resume minutes must be a whole number.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (hour == 12)
        {
            hour = 0;
        }

        if (string.Equals(periodText, "PM", StringComparison.OrdinalIgnoreCase))
        {
            hour += 12;
        }

        var time = new TimeOnly(hour, minute);

        Result = new CustomReminder
        {
            Id = _existingReminder?.Id ?? $"custom-{Guid.NewGuid():N}",
            Name = NameTextBox.Text.Trim(),
            Time = time
        };

        ResultRule = new PrayerRuleSettings
        {
            Enabled = true,
            StopBeforeMinutes = Math.Max(0, pauseBefore),
            ResumeAfterMinutes = Math.Max(0, resumeAfter),
            PlayAdhan = false,
            PlayIqama = false,
            IqamaOffsetMinutes = 0
        };

        DialogResult = true;
        Close();
    }

    private void SetSelectedTime(TimeOnly time)
    {
        var period = time.Hour >= 12 ? "PM" : "AM";
        var hour = time.Hour % 12;

        if (hour == 0)
        {
            hour = 12;
        }

        HourComboBox.SelectedItem = hour.ToString("00", CultureInfo.InvariantCulture);
        MinuteComboBox.SelectedItem = time.Minute.ToString("00", CultureInfo.InvariantCulture);
        PeriodComboBox.SelectedItem = period;
    }
}
