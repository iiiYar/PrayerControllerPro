namespace PrayerControllerPro.Core.Models;

public sealed class DailyPrayerSchedule
{
    public DateOnly Date { get; init; }

    public string CityId { get; init; } = string.Empty;

    public string? DistrictId { get; init; }

    public int CalculationMethod { get; init; }

    public string Source { get; init; } = string.Empty;

    public IReadOnlyList<PrayerScheduleEntry> Entries { get; init; } = [];
}
