using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.Tests;

public sealed class SettingsStoreTests
{
    [Fact]
    public async Task LoadAsync_UsesDefaults_WhenSettingsJsonIsInvalid()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"PrayerControllerProTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var settingsPath = Path.Combine(directory, "settings.json");
        await File.WriteAllTextAsync(settingsPath, "{ invalid json");

        var store = new SettingsStore(settingsPath, new LegacySettingsMigrator());
        var settings = await store.LoadAsync([]);

        Assert.Equal(AppCatalog.SupportedCities[0].Id, settings.SelectedCityId);
        Assert.True(File.Exists(settingsPath));
        Assert.Single(Directory.GetFiles(directory, "settings.json.invalid-*"));
    }

    [Fact]
    public async Task LoadAsync_NormalizesDefaults_WhenSettingsJsonIsEmpty()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"PrayerControllerProTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var settingsPath = Path.Combine(directory, "settings.json");
        await File.WriteAllTextAsync(settingsPath, "{}");

        var store = new SettingsStore(settingsPath, new LegacySettingsMigrator());
        var settings = await store.LoadAsync([]);

        Assert.Equal("riyadh", settings.SelectedCityId);
        Assert.Contains("Fajr", settings.PrayerRules.Keys);
        Assert.NotNull(settings.Notifications);
        Assert.True(settings.Notifications.EnableWindowsNotifications);
    }
}
