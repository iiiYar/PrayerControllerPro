namespace PrayerControllerPro.Core.Models;

public sealed class UpdateManifest
{
    public string LatestVersion { get; set; } = string.Empty;

    public string Title { get; set; } = "Update available";

    public string Notes { get; set; } = string.Empty;

    public string DownloadUrl { get; set; } = string.Empty;

    public string ReleaseUrl { get; set; } = string.Empty;

    public string? Sha256 { get; set; }

    public bool Mandatory { get; set; }
}
