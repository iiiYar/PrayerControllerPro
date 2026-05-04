namespace PrayerControllerPro.App.Services.Logging;

public sealed class AppLogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    public string Level { get; init; } = "Info";
    public string Area { get; init; } = "App";
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
}
