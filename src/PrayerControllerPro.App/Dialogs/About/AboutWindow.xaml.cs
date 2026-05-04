using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace PrayerControllerPro.App.Dialogs.About;

public partial class AboutWindow : Window
{
    public AboutWindow(DateTimeOffset? lastUpdateCheckUtc)
    {
        InitializeComponent();
        DataContext = new AboutViewModel(lastUpdateCheckUtc);

        RepositoryButton.Click += (_, _) => OpenUrl(AppIdentity.RepositoryUrl);
        CheckUpdatesButton.Click += (_, _) =>
        {
            DialogResult = true;
        };
        CloseButton.Click += (_, _) => Close();
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private sealed class AboutViewModel
    {
        public AboutViewModel(DateTimeOffset? lastUpdateCheckUtc)
        {
            LastUpdateCheckText = lastUpdateCheckUtc.HasValue
                ? lastUpdateCheckUtc.Value.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture)
                : "Never";
        }

        public string ProductName => AppIdentity.ProductName;

        public string VersionText => AppIdentity.DisplayVersion;

        public string PublisherText => AppIdentity.Publisher;

        public string PlatformText => AppIdentity.Platform;

        public string DescriptionText => AppIdentity.Description;

        public string RepositoryUrl => AppIdentity.RepositoryUrl;

        public string UpdateFeedUrl => AppIdentity.UpdateFeedUrl;

        public string LastUpdateCheckText { get; }
    }
}
