using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Tests;

public class AppCatalogTests
{
    [Fact]
    public void CreateDefaultSettings_UsesVolumeGuardAsDefaultMediaControlMode()
    {
        var settings = AppCatalog.CreateDefaultSettings();

        Assert.Equal(MediaControlMode.VolumeGuard, settings.Audio.MediaControlMode);
    }

    [Fact]
    public void EnsureDefaults_NormalizesInvalidMediaControlModeToVolumeGuard()
    {
        var settings = AppCatalog.CreateDefaultSettings();
        settings.Audio.MediaControlMode = (MediaControlMode)999;
        settings.Audio.VolumeGuardTransitionMode = (VolumeGuardTransitionMode)999;
        settings.Audio.VolumeGuardLevel = 1.5d;

        AppCatalog.EnsureDefaults(settings);

        Assert.Equal(MediaControlMode.VolumeGuard, settings.Audio.MediaControlMode);
        Assert.Equal(VolumeGuardTransitionMode.Fast, settings.Audio.VolumeGuardTransitionMode);
        Assert.Equal(1d, settings.Audio.VolumeGuardLevel);
    }

    [Fact]
    public void EnsureDefaults_PreservesValidPlayPauseKeySelection()
    {
        var settings = AppCatalog.CreateDefaultSettings();
        settings.Audio.MediaControlMode = MediaControlMode.PlayPauseKey;

        AppCatalog.EnsureDefaults(settings);

        Assert.Equal(MediaControlMode.PlayPauseKey, settings.Audio.MediaControlMode);
    }

    [Fact]
    public void SupportedDistricts_ContainsRiyadhAndJeddahSeedLists()
    {
        Assert.Equal(25, AppCatalog.GetDistrictsForCity("riyadh").Count);
        Assert.Equal(25, AppCatalog.GetDistrictsForCity("jeddah").Count);
        Assert.Empty(AppCatalog.GetDistrictsForCity("makkah"));
    }

    [Fact]
    public void EnsureDefaults_KeepsValidDistrict()
    {
        var settings = AppCatalog.CreateDefaultSettings();
        settings.SelectedCityId = "riyadh";
        settings.SelectedDistrictId = "riyadh-al-malqa";

        AppCatalog.EnsureDefaults(settings);

        Assert.Equal("riyadh-al-malqa", settings.SelectedDistrictId);
    }

    [Fact]
    public void EnsureDefaults_ClearsDistrict_WhenItDoesNotBelongToSelectedCity()
    {
        var settings = AppCatalog.CreateDefaultSettings();
        settings.SelectedCityId = "jeddah";
        settings.SelectedDistrictId = "riyadh-al-malqa";

        AppCatalog.EnsureDefaults(settings);

        Assert.Null(settings.SelectedDistrictId);
    }
}
