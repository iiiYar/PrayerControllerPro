using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Services;

public interface IPrayerTimeProvider
{
    Task<DailyPrayerSchedule> GetBuiltInScheduleAsync(CityDefinition city, int calculationMethod, CancellationToken cancellationToken = default);
}
