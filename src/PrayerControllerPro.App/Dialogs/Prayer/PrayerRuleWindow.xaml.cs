using System.Windows;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.App.Dialogs.Prayer;

public partial class PrayerRuleWindow : Window
{
    private readonly int _defaultIqamaOffset;

    public PrayerRuleWindow(string prayerName, PrayerRuleSettings currentRule, int defaultIqamaOffset)
    {
        InitializeComponent();
        _defaultIqamaOffset = defaultIqamaOffset;

        TitleTextBlock.Text = $"{prayerName} automation";
        EnabledCheckBox.IsChecked = currentRule.Enabled;
        StopBeforeTextBox.Text = currentRule.StopBeforeMinutes.ToString();
        ResumeAfterTextBox.Text = currentRule.ResumeAfterMinutes.ToString();
        IqamaOffsetTextBox.Text = (currentRule.IqamaOffsetMinutes ?? defaultIqamaOffset).ToString();
        PlayAdhanCheckBox.IsChecked = currentRule.PlayAdhan;
        PlayIqamaCheckBox.IsChecked = currentRule.PlayIqama;

        SaveButton.Click += OnSaveClicked;
        CancelButton.Click += (_, _) => Close();
    }

    public PrayerRuleSettings? Result { get; private set; }

    private void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (!int.TryParse(StopBeforeTextBox.Text, out var stopBefore)
            || !int.TryParse(ResumeAfterTextBox.Text, out var resumeAfter)
            || !int.TryParse(IqamaOffsetTextBox.Text, out var iqamaOffset))
        {
            System.Windows.MessageBox.Show(this, "Please enter valid whole numbers.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = new PrayerRuleSettings
        {
            Enabled = EnabledCheckBox.IsChecked == true,
            StopBeforeMinutes = Math.Max(0, stopBefore),
            ResumeAfterMinutes = Math.Max(0, resumeAfter),
            IqamaOffsetMinutes = Math.Max(0, iqamaOffset),
            PlayAdhan = PlayAdhanCheckBox.IsChecked == true,
            PlayIqama = PlayIqamaCheckBox.IsChecked == true
        };

        if (Result.IqamaOffsetMinutes == _defaultIqamaOffset)
        {
            Result.IqamaOffsetMinutes = null;
        }

        DialogResult = true;
        Close();
    }
}
