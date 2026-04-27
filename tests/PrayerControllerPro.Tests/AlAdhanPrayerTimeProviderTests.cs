using System.Net;
using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.Tests;

public sealed class AlAdhanPrayerTimeProviderTests
{
    [Fact]
    public async Task GetBuiltInScheduleAsync_UsesCoordinates_WhenDistrictIsSelected()
    {
        var directory = CreateTempDirectory();
        var handler = new RecordingHandler();
        var provider = new AlAdhanPrayerTimeProvider(directory, new HttpClient(handler));
        var city = AppCatalog.GetCity("riyadh");
        var district = AppCatalog.GetDistrict("riyadh", "riyadh-al-malqa");

        var schedule = await provider.GetBuiltInScheduleAsync(city, district, 4);
        var requestUrl = handler.Requests[0].OriginalString;

        Assert.NotNull(district);
        Assert.Equal("riyadh-al-malqa", schedule.DistrictId);
        Assert.Equal("API Coordinates", schedule.Source);
        Assert.Contains("/v1/timings?", requestUrl);
        Assert.Contains("latitude=24.8017", requestUrl);
        Assert.Contains("longitude=46.6053", requestUrl);
        Assert.Contains("method=4", requestUrl);
        Assert.Contains(
            Directory.GetFiles(directory, "*.json"),
            path => Path.GetFileName(path).StartsWith("district_riyadh_riyadh-al-malqa_4_", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetBuiltInScheduleAsync_UsesCityEndpoint_WhenDistrictIsNotSelected()
    {
        var directory = CreateTempDirectory();
        var handler = new RecordingHandler();
        var provider = new AlAdhanPrayerTimeProvider(directory, new HttpClient(handler));
        var city = AppCatalog.GetCity("riyadh");

        var schedule = await provider.GetBuiltInScheduleAsync(city, district: null, calculationMethod: 4);
        var requestUrl = handler.Requests[0].OriginalString;

        Assert.Null(schedule.DistrictId);
        Assert.Equal("API", schedule.Source);
        Assert.Contains("/v1/timingsByCity?", requestUrl);
        Assert.Contains("city=Riyadh", requestUrl);
        Assert.Contains("country=Saudi%20Arabia", requestUrl);
        Assert.Contains(
            Directory.GetFiles(directory, "*.json"),
            path => Path.GetFileName(path).StartsWith("city_riyadh_4_", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetBuiltInScheduleAsync_SeparatesCityAndDistrictCacheFiles()
    {
        var directory = CreateTempDirectory();
        var handler = new RecordingHandler();
        var provider = new AlAdhanPrayerTimeProvider(directory, new HttpClient(handler));
        var city = AppCatalog.GetCity("riyadh");
        var district = AppCatalog.GetDistrict("riyadh", "riyadh-al-malqa");

        await provider.GetBuiltInScheduleAsync(city, district: null, calculationMethod: 4);
        await provider.GetBuiltInScheduleAsync(city, district, calculationMethod: 4);

        var files = Directory.GetFiles(directory, "*.json").Select(Path.GetFileName).ToArray();
        Assert.Contains(files, fileName => fileName?.StartsWith("city_riyadh_4_", StringComparison.Ordinal) == true);
        Assert.Contains(files, fileName => fileName?.StartsWith("district_riyadh_riyadh-al-malqa_4_", StringComparison.Ordinal) == true);
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"PrayerControllerProTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri ?? throw new InvalidOperationException("Request URI is missing."));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                      "data": {
                        "date": {
                          "gregorian": {
                            "date": "27-04-2026"
                          }
                        },
                        "timings": {
                          "Fajr": "03:58",
                          "Sunrise": "05:16",
                          "Dhuhr": "11:51",
                          "Asr": "15:19",
                          "Sunset": "18:21",
                          "Maghrib": "18:21",
                          "Isha": "19:51",
                          "Imsak": "03:48",
                          "Midnight": "23:51",
                          "Firstthird": "21:41",
                          "Lastthird": "02:01"
                        }
                      }
                    }
                    """)
            });
        }
    }
}
