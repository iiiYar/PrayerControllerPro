namespace PrayerControllerPro.Core.Models;

public sealed class NotificationSettings
{
    public bool EnableWindowsNotifications { get; set; } = true;

    public bool EnableDiscordNotifications { get; set; }

    public string? DiscordWebhookUrl { get; set; }

    public bool NotifyOnScheduleRefresh { get; set; } = true;

    public bool NotifyOnMediaActions { get; set; } = true;

    public bool NotifyOnAudioEvents { get; set; } = true;

    public bool NotifyOnAppEvents { get; set; } = true;
}
