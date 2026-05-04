using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using PrayerControllerPro.App.Services.Logging;

namespace PrayerControllerPro.App.Services.Audio;

public sealed class AudioPresetDownloadService(string cacheDirectory, AppLogService logService) : IDisposable
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".wma",
        ".aac",
        ".m4a"
    };

    private readonly HttpClient _httpClient = new();

    public string CacheDirectory { get; } = cacheDirectory;

    public async Task<string> DownloadAsync(string presetUrl, string kind, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(presetUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException("Preset URL must be a valid HTTP or HTTPS URL.");
        }

        Directory.CreateDirectory(CacheDirectory);
        var targetPath = BuildCachePath(uri, kind);
        if (File.Exists(targetPath))
        {
            logService.Info("Audio", $"{kind} preset already exists in cache.", targetPath);
            return targetPath;
        }

        try
        {
            logService.Info("Audio", $"Downloading {kind} preset.", uri.ToString());
            using var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var temporaryPath = $"{targetPath}.tmp";
            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var destination = File.Create(temporaryPath))
            {
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
            }

            File.Move(temporaryPath, targetPath, overwrite: true);
            logService.Info("Audio", $"{kind} preset downloaded.", targetPath);
            return targetPath;
        }
        catch (Exception ex)
        {
            logService.Error("Audio", $"{kind} preset download failed.", ex);
            throw;
        }
    }

    public bool TryClearCachedFile(string? filePath)
    {
        if (!IsCachedPath(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        logService.Info("Audio", "Cached audio preset removed.", filePath);
        return true;
    }

    public bool IsCachedPath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        var cacheRoot = Path.GetFullPath(CacheDirectory);
        var candidate = Path.GetFullPath(filePath);
        return candidate.StartsWith(cacheRoot, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private string BuildCachePath(Uri uri, string kind)
    {
        var extension = Path.GetExtension(uri.LocalPath);
        if (!SupportedExtensions.Contains(extension))
        {
            extension = ".mp3";
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(uri.ToString())))[..16].ToLowerInvariant();
        var safeKind = string.Concat(kind.Where(char.IsLetterOrDigit)).ToLowerInvariant();
        return Path.Combine(CacheDirectory, $"{safeKind}-{hash}{extension}");
    }
}
