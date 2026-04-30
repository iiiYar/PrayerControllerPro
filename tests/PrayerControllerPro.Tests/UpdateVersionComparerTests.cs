using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.Tests;

public sealed class UpdateVersionComparerTests
{
    [Theory]
    [InlineData("1.1.2", true)]
    [InlineData("v1.1.2", true)]
    [InlineData("1.1.1", false)]
    [InlineData("1.1.0", false)]
    [InlineData("", false)]
    public void IsNewerVersion_ComparesManifestVersionAgainstCurrentVersion(string latestVersion, bool expected)
    {
        var currentVersion = new Version(1, 1, 1, 0);

        var isNewer = UpdateVersionComparer.IsNewerVersion(latestVersion, currentVersion);

        Assert.Equal(expected, isNewer);
    }

    [Fact]
    public void TryNormalize_IgnoresPrereleaseAndMetadataSuffixes()
    {
        var parsed = UpdateVersionComparer.TryNormalize("v1.2.3-beta+001", out var version);

        Assert.True(parsed);
        Assert.Equal(new Version(1, 2, 3, 0), version);
    }
}
