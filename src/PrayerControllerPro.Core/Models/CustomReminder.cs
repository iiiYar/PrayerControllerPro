namespace PrayerControllerPro.Core.Models;

public sealed class CustomReminder
{
    public string Id { get; set; } = $"custom-{Guid.NewGuid():N}";

    public string Name { get; set; } = string.Empty;

    public TimeOnly Time { get; set; } = new(20, 0);
}
