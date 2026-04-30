namespace PrayerControllerPro.Core.Models;

public sealed class UpdateSettings
{
    public bool CheckForUpdatesAutomatically { get; set; } = true;

    public DateTimeOffset? LastUpdateCheckUtc { get; set; }

    public string? SkippedUpdateVersion { get; set; }
}
