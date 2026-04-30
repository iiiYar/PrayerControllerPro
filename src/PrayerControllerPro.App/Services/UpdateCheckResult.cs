using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.App.Services;

public sealed record UpdateCheckResult(
    bool IsSuccess,
    bool HasUpdate,
    Version CurrentVersion,
    Version? LatestVersion,
    UpdateManifest? Manifest,
    string? ErrorMessage)
{
    public static UpdateCheckResult UpToDate(Version currentVersion, Version latestVersion)
    {
        return new UpdateCheckResult(
            IsSuccess: true,
            HasUpdate: false,
            CurrentVersion: currentVersion,
            LatestVersion: latestVersion,
            Manifest: null,
            ErrorMessage: null);
    }

    public static UpdateCheckResult UpdateAvailable(Version currentVersion, Version latestVersion, UpdateManifest manifest)
    {
        return new UpdateCheckResult(
            IsSuccess: true,
            HasUpdate: true,
            CurrentVersion: currentVersion,
            LatestVersion: latestVersion,
            Manifest: manifest,
            ErrorMessage: null);
    }

    public static UpdateCheckResult Failed(Version currentVersion, string errorMessage)
    {
        return new UpdateCheckResult(
            IsSuccess: false,
            HasUpdate: false,
            CurrentVersion: currentVersion,
            LatestVersion: null,
            Manifest: null,
            ErrorMessage: errorMessage);
    }
}
