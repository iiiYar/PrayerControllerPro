using System.Net.Http;
using System.Text;
using System.Text.Json;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.App.Services;

public sealed class NotificationService(TrayIconService trayIconService, AppLogService logService) : IDisposable
{
    private readonly HttpClient _httpClient = new();

    public async Task NotifyAsync(
        AppSettings settings,
        string title,
        string message,
        NotificationEventKind kind,
        bool force = false)
    {
        var notifications = settings.Notifications;
        if (!force && !ShouldNotify(notifications, kind))
        {
            return;
        }

        if (notifications.EnableWindowsNotifications)
        {
            trayIconService.ShowMessage(title, message);
            logService.Info("Notifications", $"Windows notification: {title}", message);
        }

        if (notifications.EnableDiscordNotifications)
        {
            await SendDiscordAsync(notifications.DiscordWebhookUrl, title, message).ConfigureAwait(false);
        }
    }

    public async Task<bool> SendDiscordTestAsync(NotificationSettings settings)
    {
        return await SendDiscordAsync(
            settings.DiscordWebhookUrl,
            AppIdentity.ProductName,
            "Discord notifications are connected.").ConfigureAwait(false);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private async Task<bool> SendDiscordAsync(string? webhookUrl, string title, string message)
    {
        if (!TryCreateDiscordWebhookUri(webhookUrl, out var uri))
        {
            logService.Warning("Notifications", "Discord webhook was skipped because the URL is empty or invalid.");
            return false;
        }

        var payload = new
        {
            username = AppIdentity.ProductName,
            content = $"**{title}**\n{message}",
            allowed_mentions = new { parse = Array.Empty<string>() }
        };

        try
        {
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                logService.Info("Notifications", $"Discord notification sent: {title}", message);
                return true;
            }

            logService.Warning(
                "Notifications",
                $"Discord notification failed with HTTP {(int)response.StatusCode}.",
                response.ReasonPhrase);
            return false;
        }
        catch (Exception ex)
        {
            logService.Error("Notifications", "Discord notification failed.", ex);
            return false;
        }
    }

    private static bool ShouldNotify(NotificationSettings settings, NotificationEventKind kind)
    {
        return kind switch
        {
            NotificationEventKind.App => settings.NotifyOnAppEvents,
            NotificationEventKind.Schedule => settings.NotifyOnScheduleRefresh,
            NotificationEventKind.Media => settings.NotifyOnMediaActions,
            NotificationEventKind.Audio => settings.NotifyOnAudioEvents,
            _ => true
        };
    }

    private static bool TryCreateDiscordWebhookUri(string? value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out uri!) && uri.Scheme == Uri.UriSchemeHttps)
        {
            return true;
        }

        uri = null!;
        return false;
    }
}
