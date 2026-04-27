using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Catalogs;

public static class AppCatalog
{
    public static IReadOnlyList<CityDefinition> SupportedCities { get; } =
    [
        new CityDefinition { Id = "riyadh", DisplayName = "Riyadh", ApiCity = "Riyadh", ApiCountry = "Saudi Arabia", TimeZoneId = "Arab Standard Time", DefaultMethod = 4 },
        new CityDefinition { Id = "jeddah", DisplayName = "Jeddah", ApiCity = "Jeddah", ApiCountry = "Saudi Arabia", TimeZoneId = "Arab Standard Time", DefaultMethod = 4 },
        new CityDefinition { Id = "makkah", DisplayName = "Makkah", ApiCity = "Makkah", ApiCountry = "Saudi Arabia", TimeZoneId = "Arab Standard Time", DefaultMethod = 4 },
        new CityDefinition { Id = "madinah", DisplayName = "Madinah", ApiCity = "Madinah", ApiCountry = "Saudi Arabia", TimeZoneId = "Arab Standard Time", DefaultMethod = 4 },
        new CityDefinition { Id = "dammam", DisplayName = "Dammam", ApiCity = "Dammam", ApiCountry = "Saudi Arabia", TimeZoneId = "Arab Standard Time", DefaultMethod = 4 },
        new CityDefinition { Id = "dubai", DisplayName = "Dubai", ApiCity = "Dubai", ApiCountry = "United Arab Emirates", TimeZoneId = "Arabian Standard Time", DefaultMethod = 8 },
        new CityDefinition { Id = "cairo", DisplayName = "Cairo", ApiCity = "Cairo", ApiCountry = "Egypt", TimeZoneId = "Egypt Standard Time", DefaultMethod = 5 }
    ];

    public static IReadOnlyList<CalculationMethodDefinition> CalculationMethods { get; } =
    [
        new CalculationMethodDefinition { Id = 4, DisplayName = "Umm Al-Qura" },
        new CalculationMethodDefinition { Id = 5, DisplayName = "Egyptian General Authority" },
        new CalculationMethodDefinition { Id = 3, DisplayName = "Muslim World League" },
        new CalculationMethodDefinition { Id = 2, DisplayName = "ISNA" },
        new CalculationMethodDefinition { Id = 1, DisplayName = "Karachi" },
        new CalculationMethodDefinition { Id = 8, DisplayName = "Gulf Region" }
    ];

    public static IReadOnlyList<PrayerDefinition> BuiltInPrayers { get; } =
    [
        new PrayerDefinition { Id = "Fajr", DisplayName = "Fajr", IconGlyph = "🌅", DefaultIqamaOffsetMinutes = 25 },
        new PrayerDefinition { Id = "Dhuhr", DisplayName = "Dhuhr", IconGlyph = "☀️", DefaultIqamaOffsetMinutes = 20 },
        new PrayerDefinition { Id = "Asr", DisplayName = "Asr", IconGlyph = "🌤️", DefaultIqamaOffsetMinutes = 20 },
        new PrayerDefinition { Id = "Maghrib", DisplayName = "Maghrib", IconGlyph = "🌇", DefaultIqamaOffsetMinutes = 10 },
        new PrayerDefinition { Id = "Isha", DisplayName = "Isha", IconGlyph = "🌙", DefaultIqamaOffsetMinutes = 20 }
    ];

    public static CityDefinition GetCity(string? cityId)
    {
        return SupportedCities.FirstOrDefault(city => string.Equals(city.Id, cityId, StringComparison.OrdinalIgnoreCase))
            ?? SupportedCities[0];
    }

    public static PrayerDefinition GetBuiltInPrayer(string prayerId)
    {
        return BuiltInPrayers.First(prayer => string.Equals(prayer.Id, prayerId, StringComparison.OrdinalIgnoreCase));
    }

    public static AppSettings CreateDefaultSettings()
    {
        var settings = new AppSettings();
        EnsureDefaults(settings);
        return settings;
    }

    public static void EnsureDefaults(AppSettings settings)
    {
        settings.Audio ??= new AudioSettings();
        settings.Notifications ??= new NotificationSettings();
        settings.PrayerRules ??= new Dictionary<string, PrayerRuleSettings>(StringComparer.OrdinalIgnoreCase);
        settings.CustomReminders ??= [];

        settings.Audio.AdhanAudioPath = NormalizeOptionalText(settings.Audio.AdhanAudioPath);
        settings.Audio.IqamaAudioPath = NormalizeOptionalText(settings.Audio.IqamaAudioPath);
        settings.Audio.AdhanPresetUrl = NormalizeOptionalText(settings.Audio.AdhanPresetUrl);
        settings.Audio.IqamaPresetUrl = NormalizeOptionalText(settings.Audio.IqamaPresetUrl);
        settings.Audio.Volume = Math.Clamp(settings.Audio.Volume, 0d, 1d);
        settings.Audio.VolumeGuardLevel = Math.Clamp(settings.Audio.VolumeGuardLevel, 0d, 1d);
        if (!Enum.IsDefined(settings.Audio.MediaControlMode))
        {
            settings.Audio.MediaControlMode = MediaControlMode.PlayPauseKey;
        }

        settings.Notifications.DiscordWebhookUrl = NormalizeOptionalText(settings.Notifications.DiscordWebhookUrl);

        if (string.IsNullOrWhiteSpace(settings.SelectedCityId))
        {
            settings.SelectedCityId = SupportedCities[0].Id;
        }

        if (!CalculationMethods.Any(method => method.Id == settings.CalculationMethod))
        {
            settings.CalculationMethod = GetCity(settings.SelectedCityId).DefaultMethod;
        }

        foreach (var prayer in BuiltInPrayers)
        {
            if (!settings.PrayerRules.TryGetValue(prayer.Id, out var rule))
            {
                settings.PrayerRules[prayer.Id] = CreateDefaultRule(prayer.IsCustom);
                continue;
            }

            NormalizeRule(rule, prayer.IsCustom);
        }

        foreach (var reminder in settings.CustomReminders.Where(reminder => !string.IsNullOrWhiteSpace(reminder.Name)))
        {
            if (!settings.PrayerRules.TryGetValue(reminder.Id, out var rule))
            {
                settings.PrayerRules[reminder.Id] = CreateDefaultRule(isCustom: true);
                continue;
            }

            NormalizeRule(rule, isCustom: true);
        }
    }

    public static PrayerRuleSettings CreateDefaultRule(bool isCustom)
    {
        if (isCustom)
        {
            return new PrayerRuleSettings
            {
                Enabled = true,
                StopBeforeMinutes = 0,
                ResumeAfterMinutes = 5,
                PlayAdhan = false,
                PlayIqama = false,
                IqamaOffsetMinutes = 0
            };
        }

        return new PrayerRuleSettings
        {
            Enabled = true,
            StopBeforeMinutes = 5,
            ResumeAfterMinutes = 15,
            PlayAdhan = true,
            PlayIqama = false
        };
    }

    private static void NormalizeRule(PrayerRuleSettings rule, bool isCustom)
    {
        rule.StopBeforeMinutes = Math.Max(0, rule.StopBeforeMinutes);
        rule.ResumeAfterMinutes = Math.Max(0, rule.ResumeAfterMinutes);

        if (rule.IqamaOffsetMinutes is < 0)
        {
            rule.IqamaOffsetMinutes = 0;
        }

        if (isCustom && rule.IqamaOffsetMinutes is null)
        {
            rule.IqamaOffsetMinutes = 0;
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
