namespace PrayerControllerPro.Core.Models;

public sealed class CountdownInfo
{
    public CountdownMode Mode { get; init; }

    public string PrayerId { get; init; } = string.Empty;

    public string PrayerName { get; init; } = string.Empty;

    public DateTimeOffset TargetTime { get; init; }

    public TimeSpan Remaining { get; init; }
}
