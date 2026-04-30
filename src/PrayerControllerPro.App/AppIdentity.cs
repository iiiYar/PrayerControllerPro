using System.Reflection;

namespace PrayerControllerPro.App;

internal static class AppIdentity
{
    public const string ProductName = "Prayer Controller Pro";
    public const string Publisher = "iiiYar";
    public const string Platform = "Windows desktop app on .NET 8 / WPF";
    public const string Description = "Prayer timing automation and media control for Windows.";
    public const string RepositoryUrl = "https://github.com/iiiYar/PrayerControllerPro";
    public const string UpdateFeedUrl = "https://raw.githubusercontent.com/iiiYar/PrayerControllerPro/main/update.json";

    public static Version CurrentVersion { get; } =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

    public static string DisplayVersion => CurrentVersion.ToString(3);
}
