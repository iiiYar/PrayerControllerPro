using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Services;

public sealed class PrayerScheduleComposer
{
    public DailyPrayerSchedule Compose(DailyPrayerSchedule builtInSchedule, AppSettings settings, CityDefinition city)
    {
        AppCatalog.EnsureDefaults(settings);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(city.TimeZoneId);
        var entries = new List<PrayerScheduleEntry>(builtInSchedule.Entries.Count + settings.CustomReminders.Count);

        foreach (var entry in builtInSchedule.Entries)
        {
            var rule = settings.PrayerRules[entry.Id].Clone();
            var iqamaOffset = rule.IqamaOffsetMinutes ?? AppCatalog.GetBuiltInPrayer(entry.Id).DefaultIqamaOffsetMinutes;

            entries.Add(new PrayerScheduleEntry
            {
                Id = entry.Id,
                DisplayName = entry.DisplayName,
                IconGlyph = entry.IconGlyph,
                IsCustom = false,
                PrayerTime = entry.PrayerTime,
                IqamaTime = entry.PrayerTime.AddMinutes(iqamaOffset),
                Rule = rule
            });
        }

        foreach (var reminder in settings.CustomReminders.Where(reminder => !string.IsNullOrWhiteSpace(reminder.Name)))
        {
            if (!settings.PrayerRules.TryGetValue(reminder.Id, out var storedRule))
            {
                storedRule = AppCatalog.CreateDefaultRule(isCustom: true);
                settings.PrayerRules[reminder.Id] = storedRule;
            }

            var rule = storedRule.Clone();
            var prayerLocalTime = builtInSchedule.Date.ToDateTime(reminder.Time, DateTimeKind.Unspecified);
            var prayerTime = new DateTimeOffset(prayerLocalTime, timeZone.GetUtcOffset(prayerLocalTime));
            var iqamaTime = prayerTime.AddMinutes(rule.IqamaOffsetMinutes ?? 0);

            entries.Add(new PrayerScheduleEntry
            {
                Id = reminder.Id,
                DisplayName = reminder.Name,
                IconGlyph = "📌",
                IsCustom = true,
                PrayerTime = prayerTime,
                IqamaTime = iqamaTime,
                Rule = rule
            });
        }

        return new DailyPrayerSchedule
        {
            Date = builtInSchedule.Date,
            CityId = builtInSchedule.CityId,
            DistrictId = builtInSchedule.DistrictId,
            CalculationMethod = builtInSchedule.CalculationMethod,
            Source = builtInSchedule.Source,
            Entries = entries.OrderBy(entry => entry.PrayerTime).ToList()
        };
    }
}
