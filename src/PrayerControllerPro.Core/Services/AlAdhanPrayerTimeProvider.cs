using System.Globalization;
using System.Text.Json;
using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Services;

public sealed class AlAdhanPrayerTimeProvider(string cacheDirectory, HttpClient? httpClient = null) : IPrayerTimeProvider
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

    public async Task<DailyPrayerSchedule> GetBuiltInScheduleAsync(CityDefinition city, int calculationMethod, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(cacheDirectory);

        var cityNow = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, ResolveTimeZone(city));
        var cityDate = DateOnly.FromDateTime(cityNow.Date);
        var cachePath = Path.Combine(cacheDirectory, $"{city.Id}_{calculationMethod}_{cityDate:yyyyMMdd}.json");

        if (File.Exists(cachePath))
        {
            try
            {
                var cachedJson = await File.ReadAllTextAsync(cachePath, cancellationToken).ConfigureAwait(false);
                return ParseSchedule(cachedJson, city, calculationMethod, "Cache");
            }
            catch
            {
                // Ignore broken cache and fetch a fresh copy.
            }
        }

        try
        {
            var url =
                $"https://api.aladhan.com/v1/timingsByCity?city={Uri.EscapeDataString(city.ApiCity)}&country={Uri.EscapeDataString(city.ApiCountry)}&method={calculationMethod}";
            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(cachePath, json, cancellationToken).ConfigureAwait(false);
            return ParseSchedule(json, city, calculationMethod, "API");
        }
        catch
        {
            if (File.Exists(cachePath))
            {
                var cachedJson = await File.ReadAllTextAsync(cachePath, cancellationToken).ConfigureAwait(false);
                return ParseSchedule(cachedJson, city, calculationMethod, "Cache");
            }

            throw;
        }
    }

    private static DailyPrayerSchedule ParseSchedule(string json, CityDefinition city, int calculationMethod, string source)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var data = root.GetProperty("data");
        var dateText = data.GetProperty("date").GetProperty("gregorian").GetProperty("date").GetString()
            ?? throw new InvalidOperationException("Gregorian date is missing from API response.");
        var date = DateOnly.ParseExact(dateText, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        var timings = data.GetProperty("timings");
        var timeZone = ResolveTimeZone(city);

        var entries = AppCatalog.BuiltInPrayers
            .Select(prayer =>
            {
                var prayerTime = CreateOffsetDateTime(date, ParseTime(timings.GetProperty(prayer.Id).GetString()), timeZone);
                var iqamaTime = prayerTime.AddMinutes(prayer.DefaultIqamaOffsetMinutes);

                return new PrayerScheduleEntry
                {
                    Id = prayer.Id,
                    DisplayName = prayer.DisplayName,
                    IconGlyph = prayer.IconGlyph,
                    IsCustom = false,
                    PrayerTime = prayerTime,
                    IqamaTime = iqamaTime,
                    Rule = AppCatalog.CreateDefaultRule(isCustom: false)
                };
            })
            .OrderBy(entry => entry.PrayerTime)
            .ToList();

        return new DailyPrayerSchedule
        {
            Date = date,
            CityId = city.Id,
            CalculationMethod = calculationMethod,
            Source = source,
            Entries = entries
        };
    }

    private static TimeOnly ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Prayer time value is missing.");
        }

        var token = value.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        if (TimeOnly.TryParseExact(token, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            return exact;
        }

        if (TimeOnly.TryParseExact(token, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var shortForm))
        {
            return shortForm;
        }

        throw new InvalidOperationException($"Unable to parse prayer time '{value}'.");
    }

    private static DateTimeOffset CreateOffsetDateTime(DateOnly date, TimeOnly time, TimeZoneInfo timeZone)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
    }

    private static TimeZoneInfo ResolveTimeZone(CityDefinition city)
    {
        return TimeZoneInfo.FindSystemTimeZoneById(city.TimeZoneId);
    }
}
