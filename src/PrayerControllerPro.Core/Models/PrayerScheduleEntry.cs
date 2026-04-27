namespace PrayerControllerPro.Core.Models;

public sealed class PrayerScheduleEntry
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string IconGlyph { get; init; } = string.Empty;

    public bool IsCustom { get; init; }

    public DateTimeOffset PrayerTime { get; init; }

    public DateTimeOffset IqamaTime { get; init; }

    public PrayerRuleSettings Rule { get; init; } = new();
}
