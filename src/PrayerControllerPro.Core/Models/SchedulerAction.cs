namespace PrayerControllerPro.Core.Models;

public sealed class SchedulerAction
{
    public SchedulerActionKind Kind { get; init; }

    public string PrayerId { get; init; } = string.Empty;

    public string PrayerName { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
