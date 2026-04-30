namespace PrayerControllerPro.Core.Services;

public static class UpdateVersionComparer
{
    public static bool IsNewerVersion(string? latestVersionText, Version currentVersion)
    {
        return TryNormalize(latestVersionText, out var latestVersion)
            && Normalize(currentVersion).CompareTo(latestVersion) < 0;
    }

    public static bool TryNormalize(string? versionText, out Version version)
    {
        version = new Version(0, 0, 0, 0);
        if (string.IsNullOrWhiteSpace(versionText))
        {
            return false;
        }

        var normalizedText = versionText.Trim().TrimStart('v', 'V');
        var metadataIndex = normalizedText.IndexOfAny(['-', '+']);
        if (metadataIndex >= 0)
        {
            normalizedText = normalizedText[..metadataIndex];
        }

        if (!Version.TryParse(normalizedText, out var parsed))
        {
            return false;
        }

        version = Normalize(parsed);
        return true;
    }

    private static Version Normalize(Version version)
    {
        return new Version(
            Math.Max(version.Major, 0),
            Math.Max(version.Minor, 0),
            Math.Max(version.Build, 0),
            Math.Max(version.Revision, 0));
    }
}
