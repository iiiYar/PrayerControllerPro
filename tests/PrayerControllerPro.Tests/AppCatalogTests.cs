using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Tests;

public class AppCatalogTests
{
    [Fact]
    public void EnsureDefaults_NormalizesVolumeGuardSettings()
    {
        var settings = AppCatalog.CreateDefaultSettings();
        settings.Audio.MediaControlMode = (MediaControlMode)999;
        settings.Audio.VolumeGuardLevel = 1.5d;

        AppCatalog.EnsureDefaults(settings);

        Assert.Equal(MediaControlMode.PlayPauseKey, settings.Audio.MediaControlMode);
        Assert.Equal(1d, settings.Audio.VolumeGuardLevel);
    }
}
