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

    public static IReadOnlyList<DistrictDefinition> SupportedDistricts { get; } =
    [
        new DistrictDefinition { Id = "riyadh-al-malqa", CityId = "riyadh", DisplayName = "Al Malqa", Latitude = 24.8017, Longitude = 46.6053 },
        new DistrictDefinition { Id = "riyadh-al-narjis", CityId = "riyadh", DisplayName = "Al Narjis", Latitude = 24.8393, Longitude = 46.6734 },
        new DistrictDefinition { Id = "riyadh-al-yasmin", CityId = "riyadh", DisplayName = "Al Yasmin", Latitude = 24.8276, Longitude = 46.6316 },
        new DistrictDefinition { Id = "riyadh-al-arid", CityId = "riyadh", DisplayName = "Al Arid", Latitude = 24.8910, Longitude = 46.6060 },
        new DistrictDefinition { Id = "riyadh-hittin", CityId = "riyadh", DisplayName = "Hittin", Latitude = 24.7651, Longitude = 46.6012 },
        new DistrictDefinition { Id = "riyadh-al-aqiq", CityId = "riyadh", DisplayName = "Al Aqiq", Latitude = 24.7772, Longitude = 46.6300 },
        new DistrictDefinition { Id = "riyadh-al-ghadeer", CityId = "riyadh", DisplayName = "Al Ghadeer", Latitude = 24.7752, Longitude = 46.6612 },
        new DistrictDefinition { Id = "riyadh-al-mursalat", CityId = "riyadh", DisplayName = "Al Mursalat", Latitude = 24.7538, Longitude = 46.6810 },
        new DistrictDefinition { Id = "riyadh-al-wadi", CityId = "riyadh", DisplayName = "Al Wadi", Latitude = 24.7890, Longitude = 46.6830 },
        new DistrictDefinition { Id = "riyadh-al-rabwah", CityId = "riyadh", DisplayName = "Al Rabwah", Latitude = 24.6888, Longitude = 46.7575 },
        new DistrictDefinition { Id = "riyadh-al-rawdah", CityId = "riyadh", DisplayName = "Al Rawdah", Latitude = 24.7340, Longitude = 46.7667 },
        new DistrictDefinition { Id = "riyadh-al-rayyan", CityId = "riyadh", DisplayName = "Al Rayyan", Latitude = 24.7046, Longitude = 46.7816 },
        new DistrictDefinition { Id = "riyadh-al-sulaymaniyah", CityId = "riyadh", DisplayName = "Al Sulaymaniyah", Latitude = 24.7002, Longitude = 46.6966 },
        new DistrictDefinition { Id = "riyadh-al-olaya", CityId = "riyadh", DisplayName = "Al Olaya", Latitude = 24.7110, Longitude = 46.6720 },
        new DistrictDefinition { Id = "riyadh-al-muruj", CityId = "riyadh", DisplayName = "Al Muruj", Latitude = 24.7600, Longitude = 46.6610 },
        new DistrictDefinition { Id = "riyadh-al-nakheel", CityId = "riyadh", DisplayName = "Al Nakheel", Latitude = 24.7473, Longitude = 46.6311 },
        new DistrictDefinition { Id = "riyadh-al-qirawan", CityId = "riyadh", DisplayName = "Al Qirawan", Latitude = 24.8230, Longitude = 46.5570 },
        new DistrictDefinition { Id = "riyadh-al-sahafah", CityId = "riyadh", DisplayName = "Al Sahafah", Latitude = 24.7980, Longitude = 46.6430 },
        new DistrictDefinition { Id = "riyadh-king-fahd", CityId = "riyadh", DisplayName = "King Fahd", Latitude = 24.7406, Longitude = 46.6817 },
        new DistrictDefinition { Id = "riyadh-al-izdihar", CityId = "riyadh", DisplayName = "Al Izdihar", Latitude = 24.7850, Longitude = 46.7250 },
        new DistrictDefinition { Id = "riyadh-al-hamra", CityId = "riyadh", DisplayName = "Al Hamra", Latitude = 24.7759, Longitude = 46.7658 },
        new DistrictDefinition { Id = "riyadh-al-shifa", CityId = "riyadh", DisplayName = "Al Shifa", Latitude = 24.5757, Longitude = 46.7064 },
        new DistrictDefinition { Id = "riyadh-al-aziziyah", CityId = "riyadh", DisplayName = "Al Aziziyah", Latitude = 24.5933, Longitude = 46.7728 },
        new DistrictDefinition { Id = "riyadh-al-nuzhah", CityId = "riyadh", DisplayName = "Al Nuzhah", Latitude = 24.7544, Longitude = 46.7087 },
        new DistrictDefinition { Id = "riyadh-al-wurud", CityId = "riyadh", DisplayName = "Al Wurud", Latitude = 24.7281, Longitude = 46.6717 },

        new DistrictDefinition { Id = "jeddah-al-hamra", CityId = "jeddah", DisplayName = "Al Hamra", Latitude = 21.5266, Longitude = 39.1663 },
        new DistrictDefinition { Id = "jeddah-al-rawdah", CityId = "jeddah", DisplayName = "Al Rawdah", Latitude = 21.5667, Longitude = 39.1608 },
        new DistrictDefinition { Id = "jeddah-al-zahra", CityId = "jeddah", DisplayName = "Al Zahra", Latitude = 21.5890, Longitude = 39.1650 },
        new DistrictDefinition { Id = "jeddah-al-salamah", CityId = "jeddah", DisplayName = "Al Salamah", Latitude = 21.5953, Longitude = 39.1836 },
        new DistrictDefinition { Id = "jeddah-al-naeem", CityId = "jeddah", DisplayName = "Al Naeem", Latitude = 21.6240, Longitude = 39.1570 },
        new DistrictDefinition { Id = "jeddah-al-marwah", CityId = "jeddah", DisplayName = "Al Marwah", Latitude = 21.6115, Longitude = 39.2190 },
        new DistrictDefinition { Id = "jeddah-al-safa", CityId = "jeddah", DisplayName = "Al Safa", Latitude = 21.5790, Longitude = 39.2174 },
        new DistrictDefinition { Id = "jeddah-al-bawadi", CityId = "jeddah", DisplayName = "Al Bawadi", Latitude = 21.5895, Longitude = 39.1955 },
        new DistrictDefinition { Id = "jeddah-al-faisaliyah", CityId = "jeddah", DisplayName = "Al Faisaliyah", Latitude = 21.5600, Longitude = 39.1990 },
        new DistrictDefinition { Id = "jeddah-al-andalus", CityId = "jeddah", DisplayName = "Al Andalus", Latitude = 21.5390, Longitude = 39.1625 },
        new DistrictDefinition { Id = "jeddah-al-khalidiyah", CityId = "jeddah", DisplayName = "Al Khalidiyah", Latitude = 21.5580, Longitude = 39.1520 },
        new DistrictDefinition { Id = "jeddah-al-shati", CityId = "jeddah", DisplayName = "Al Shati", Latitude = 21.6014, Longitude = 39.1102 },
        new DistrictDefinition { Id = "jeddah-al-nahdah", CityId = "jeddah", DisplayName = "Al Nahdah", Latitude = 21.6180, Longitude = 39.1450 },
        new DistrictDefinition { Id = "jeddah-al-muhammadiyah", CityId = "jeddah", DisplayName = "Al Muhammadiyah", Latitude = 21.6225, Longitude = 39.1610 },
        new DistrictDefinition { Id = "jeddah-al-basateen", CityId = "jeddah", DisplayName = "Al Basateen", Latitude = 21.6735, Longitude = 39.1610 },
        new DistrictDefinition { Id = "jeddah-obhur-al-shamaliyah", CityId = "jeddah", DisplayName = "Obhur Al Shamaliyah", Latitude = 21.7540, Longitude = 39.1110 },
        new DistrictDefinition { Id = "jeddah-al-murjan", CityId = "jeddah", DisplayName = "Al Murjan", Latitude = 21.6465, Longitude = 39.1145 },
        new DistrictDefinition { Id = "jeddah-al-rehab", CityId = "jeddah", DisplayName = "Al Rehab", Latitude = 21.5490, Longitude = 39.2310 },
        new DistrictDefinition { Id = "jeddah-al-aziziyah", CityId = "jeddah", DisplayName = "Al Aziziyah", Latitude = 21.5530, Longitude = 39.2060 },
        new DistrictDefinition { Id = "jeddah-al-nuzhah", CityId = "jeddah", DisplayName = "Al Nuzhah", Latitude = 21.6200, Longitude = 39.1925 },
        new DistrictDefinition { Id = "jeddah-al-samer", CityId = "jeddah", DisplayName = "Al Samer", Latitude = 21.6120, Longitude = 39.2450 },
        new DistrictDefinition { Id = "jeddah-al-ajwad", CityId = "jeddah", DisplayName = "Al Ajwad", Latitude = 21.5750, Longitude = 39.2820 },
        new DistrictDefinition { Id = "jeddah-al-rabwah", CityId = "jeddah", DisplayName = "Al Rabwah", Latitude = 21.5960, Longitude = 39.2050 },
        new DistrictDefinition { Id = "jeddah-al-faihaa", CityId = "jeddah", DisplayName = "Al Faihaa", Latitude = 21.4895, Longitude = 39.2220 },
        new DistrictDefinition { Id = "jeddah-al-naseem", CityId = "jeddah", DisplayName = "Al Naseem", Latitude = 21.5105, Longitude = 39.2390 }
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

    public static IReadOnlyList<DistrictDefinition> GetDistrictsForCity(string? cityId)
    {
        return SupportedDistricts
            .Where(district => string.Equals(district.CityId, cityId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(district => district.DisplayName)
            .ToList();
    }

    public static DistrictDefinition? GetDistrict(string? cityId, string? districtId)
    {
        if (string.IsNullOrWhiteSpace(districtId))
        {
            return null;
        }

        return SupportedDistricts.FirstOrDefault(district =>
            string.Equals(district.CityId, cityId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(district.Id, districtId, StringComparison.OrdinalIgnoreCase));
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
        settings.Updates ??= new UpdateSettings();
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

        if (!Enum.IsDefined(settings.Audio.VolumeGuardTransitionMode))
        {
            settings.Audio.VolumeGuardTransitionMode = VolumeGuardTransitionMode.Fast;
        }

        settings.Notifications.DiscordWebhookUrl = NormalizeOptionalText(settings.Notifications.DiscordWebhookUrl);
        settings.Updates.SkippedUpdateVersion = NormalizeOptionalText(settings.Updates.SkippedUpdateVersion);

        if (string.IsNullOrWhiteSpace(settings.SelectedCityId)
            || !SupportedCities.Any(city => string.Equals(city.Id, settings.SelectedCityId, StringComparison.OrdinalIgnoreCase)))
        {
            settings.SelectedCityId = SupportedCities[0].Id;
        }

        settings.SelectedDistrictId = NormalizeOptionalText(settings.SelectedDistrictId);
        if (GetDistrict(settings.SelectedCityId, settings.SelectedDistrictId) is null)
        {
            settings.SelectedDistrictId = null;
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
