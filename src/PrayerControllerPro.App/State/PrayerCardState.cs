using System.Globalization;
using PrayerControllerPro.App.Infrastructure;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.App.State;

public sealed class PrayerCardState : ObservableObject
{
    private bool _isNext;
    private bool _isActive;

    public PrayerCardState(PrayerScheduleEntry entry)
    {
        Id = entry.Id;
        DisplayName = entry.DisplayName;
        IconGlyph = entry.IconGlyph;
        TimeText = entry.PrayerTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        IqamaText = entry.IqamaTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        IsCustom = entry.IsCustom;
        IsEnabled = entry.Rule.Enabled;
        RuleSummary = entry.Rule.Enabled
            ? $"Auto: -{entry.Rule.StopBeforeMinutes}m / +{entry.Rule.ResumeAfterMinutes}m"
            : "Automation is off";
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string IconGlyph { get; }

    public string TimeText { get; }

    public string IqamaText { get; }

    public bool IsCustom { get; }

    public bool IsEnabled { get; }

    public string RuleSummary { get; }

    public bool IsNext
    {
        get => _isNext;
        set => SetProperty(ref _isNext, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}
