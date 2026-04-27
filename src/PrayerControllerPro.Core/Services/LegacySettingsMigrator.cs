using System.Globalization;
using System.Text.Json;
using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Services;

public sealed class LegacySettingsMigrator
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AppSettings? TryMigrate(IEnumerable<string> candidateDirectories)
    {
        foreach (var directory in candidateDirectories.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var appSettingsPath = Path.Combine(directory, "app_settings.json");
            var prayerSettingsPath = Path.Combine(directory, "prayer_settings.json");
            var customPrayersPath = Path.Combine(directory, "custom_prayers.json");

            if (!File.Exists(appSettingsPath) && !File.Exists(prayerSettingsPath) && !File.Exists(customPrayersPath))
            {
                continue;
            }

            var migrated = AppCatalog.CreateDefaultSettings();

            if (File.Exists(appSettingsPath))
            {
                var legacyApp = JsonSerializer.Deserialize<LegacyAppSettings>(File.ReadAllText(appSettingsPath), _jsonOptions);
                if (legacyApp is not null)
                {
                    migrated.SelectedCityId = MapCityId(legacyApp.City);
                    migrated.CalculationMethod = legacyApp.Method > 0 ? legacyApp.Method : AppCatalog.GetCity(migrated.SelectedCityId).DefaultMethod;
                    migrated.AutoStart = legacyApp.AutoStart;
                    migrated.Theme = string.IsNullOrWhiteSpace(legacyApp.Theme) ? "Dark" : legacyApp.Theme;
                }
            }

            if (File.Exists(customPrayersPath))
            {
                var legacyReminders = JsonSerializer.Deserialize<List<LegacyCustomReminder>>(File.ReadAllText(customPrayersPath), _jsonOptions) ?? [];
                foreach (var legacyReminder in legacyReminders.Where(item => !string.IsNullOrWhiteSpace(item.Name)))
                {
                    if (!TryParseLegacyTime(legacyReminder.TimeStr, out var reminderTime))
                    {
                        continue;
                    }

                    var reminder = new CustomReminder
                    {
                        Id = $"custom-{NormalizeName(legacyReminder.Name!)}",
                        Name = legacyReminder.Name!.Trim(),
                        Time = reminderTime
                    };

                    migrated.CustomReminders.Add(reminder);
                    migrated.PrayerRules[reminder.Id] = AppCatalog.CreateDefaultRule(isCustom: true);
                }
            }

            if (File.Exists(prayerSettingsPath))
            {
                var legacyRules = JsonSerializer.Deserialize<Dictionary<string, LegacyPrayerRule>>(File.ReadAllText(prayerSettingsPath), _jsonOptions)
                    ?? new Dictionary<string, LegacyPrayerRule>(StringComparer.OrdinalIgnoreCase);

                foreach (var pair in legacyRules)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value is null)
                    {
                        continue;
                    }

                    var targetKey = ResolveRuleKey(pair.Key, migrated.CustomReminders);
                    migrated.PrayerRules[targetKey] = new PrayerRuleSettings
                    {
                        Enabled = pair.Value.Enabled,
                        StopBeforeMinutes = Math.Max(0, pair.Value.MinutesBefore),
                        ResumeAfterMinutes = Math.Max(0, pair.Value.MinutesAfter),
                        PlayAdhan = !pair.Value.HasStoppedToday,
                        PlayIqama = false,
                        IqamaOffsetMinutes = targetKey.StartsWith("custom-", StringComparison.OrdinalIgnoreCase) ? 0 : null
                    };
                }
            }

            AppCatalog.EnsureDefaults(migrated);
            return migrated;
        }

        return null;
    }

    private static string MapCityId(string? cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
        {
            return AppCatalog.SupportedCities[0].Id;
        }

        var match = AppCatalog.SupportedCities.FirstOrDefault(city =>
            string.Equals(city.ApiCity, cityName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(city.DisplayName, cityName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(city.Id, cityName, StringComparison.OrdinalIgnoreCase));

        return match?.Id ?? AppCatalog.SupportedCities[0].Id;
    }

    private static bool TryParseLegacyTime(string? value, out TimeOnly result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
            || TimeOnly.TryParseExact(value, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    private static string ResolveRuleKey(string legacyKey, IReadOnlyCollection<CustomReminder> reminders)
    {
        var builtIn = AppCatalog.BuiltInPrayers.FirstOrDefault(prayer => string.Equals(prayer.Id, legacyKey, StringComparison.OrdinalIgnoreCase));
        if (builtIn is not null)
        {
            return builtIn.Id;
        }

        var reminder = reminders.FirstOrDefault(item => string.Equals(item.Name, legacyKey, StringComparison.OrdinalIgnoreCase));
        return reminder?.Id ?? $"custom-{NormalizeName(legacyKey)}";
    }

    private static string NormalizeName(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();

        return string.Join(string.Empty, chars).Trim('-');
    }

    private sealed class LegacyAppSettings
    {
        public string? City { get; set; }

        public int Method { get; set; }

        public bool AutoStart { get; set; }

        public string? Theme { get; set; }
    }

    private sealed class LegacyPrayerRule
    {
        public bool Enabled { get; set; } = true;

        public int MinutesBefore { get; set; } = 5;

        public int MinutesAfter { get; set; } = 15;

        public bool HasStoppedToday { get; set; }
    }

    private sealed class LegacyCustomReminder
    {
        public string? Name { get; set; }

        public string? TimeStr { get; set; }
    }
}
