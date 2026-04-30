using System.Net.Http;
using System.Text.Json;
using PrayerControllerPro.Core.Models;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.App.Services;

public sealed class UpdateCheckService : IDisposable
{
    public const string DefaultManifestUrl = "https://raw.githubusercontent.com/iiiYar/PrayerControllerPro/main/update.json";

    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AppLogService _logService;

    public UpdateCheckService(AppLogService logService, Version currentVersion)
    {
        _logService = logService;
        CurrentVersion = Normalize(currentVersion);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"PrayerControllerPro/{CurrentVersion}");
    }

    public Version CurrentVersion { get; }

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await _httpClient.GetStringAsync(DefaultManifestUrl, cancellationToken).ConfigureAwait(false);
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

    public void Dispose()
    {
        _httpClient.Dispose();
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
