namespace PrayerControllerPro.Core.Models;

public sealed class AppSettings
{
    public string SelectedCityId { get; set; } = "riyadh";

    public int CalculationMethod { get; set; } = 4;

    public bool AutoStart { get; set; }

    public string Theme { get; set; } = "Dark";

    public AudioSettings Audio { get; set; } = new();

    public NotificationSettings Notifications { get; set; } = new();

    public Dictionary<string, PrayerRuleSettings> PrayerRules { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<CustomReminder> CustomReminders { get; set; } = [];
}
