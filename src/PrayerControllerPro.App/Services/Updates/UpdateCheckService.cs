using System.Text.Json;
using PrayerControllerPro.App.Services.Logging;
using PrayerControllerPro.App.Services.System;
using PrayerControllerPro.Core.Models;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.App.Services.Updates;

public sealed class UpdateCheckService
{
    public const string DefaultManifestUrl = AppIdentity.UpdateFeedUrl;

    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AppHttpClient _appHttpClient;
    private readonly AppLogService _logService;

    public UpdateCheckService(
        AppLogService logService,
        AppHttpClient appHttpClient,
        Version currentVersion)
    {
        _logService = logService;
        _appHttpClient = appHttpClient;
        CurrentVersion = Normalize(currentVersion);
    }

    public Version CurrentVersion { get; }

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await _appHttpClient.GetStringAsync(DefaultManifestUrl, cancellationToken).ConfigureAwait(false);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(json, _jsonOptions);
            if (manifest is null)
            {
                return UpdateCheckResult.Failed(CurrentVersion, "Update manifest is empty.");
            }

            if (!UpdateVersionComparer.TryNormalize(manifest.LatestVersion, out var latestVersion))
            {
                return UpdateCheckResult.Failed(CurrentVersion, "Update manifest has an invalid latestVersion.");
            }

            if (!IsValidHttpsUrl(manifest.DownloadUrl) && !IsValidHttpsUrl(manifest.ReleaseUrl))
            {
                return UpdateCheckResult.Failed(CurrentVersion, "Update manifest does not contain a valid HTTPS download or release URL.");
            }

            if (!UpdateVersionComparer.IsNewerVersion(manifest.LatestVersion, CurrentVersion))
            {
                _logService.Info("Updates", "Application is up to date.", $"Current={CurrentVersion}; Latest={latestVersion}");
                return UpdateCheckResult.UpToDate(CurrentVersion, latestVersion);
            }

            _logService.Info("Updates", "Update available.", $"Current={CurrentVersion}; Latest={latestVersion}");
            return UpdateCheckResult.UpdateAvailable(CurrentVersion, latestVersion, manifest);
        }
        catch (Exception ex)
        {
            _logService.Warning("Updates", "Update check failed.", ex.Message);
            return UpdateCheckResult.Failed(CurrentVersion, ex.Message);
        }
    }

    private static bool IsValidHttpsUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;
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
