using System.Diagnostics;
using System.Windows;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.App.Dialogs.Update;

public partial class UpdateAvailableWindow : Window
{
    private readonly UpdateManifest _manifest;

    public UpdateAvailableWindow(UpdateManifest manifest, Version currentVersion)
    {
        InitializeComponent();
        _manifest = manifest;

        TitleTextBlock.Text = string.IsNullOrWhiteSpace(manifest.Title)
            ? "Update available"
            : manifest.Title;
        VersionTextBlock.Text = $"Current version: {currentVersion.ToString(3)}  |  Latest version: {manifest.LatestVersion}";
        NotesTextBox.Text = string.IsNullOrWhiteSpace(manifest.Notes)
            ? "No release notes were provided."
            : manifest.Notes;
        MandatoryTextBlock.Text = manifest.Mandatory ? "Required update" : string.Empty;
        SkipButton.Visibility = manifest.Mandatory ? Visibility.Collapsed : Visibility.Visible;

        DownloadButton.Click += (_, _) => OpenAndClose(GetDownloadTargetUrl());
        ReleaseNotesButton.Click += (_, _) => OpenUrl(manifest.ReleaseUrl);
        LaterButton.Click += (_, _) => Close();
        SkipButton.Click += (_, _) =>
        {
            SkipVersion = true;
            Close();
        };
    }

    public bool SkipVersion { get; private set; }

    private void OpenAndClose(string url)
    {
        if (OpenUrl(url))
        {
            Close();
        }
    }

    private string GetDownloadTargetUrl()
    {
        return IsValidHttpsUrl(_manifest.DownloadUrl)
            ? _manifest.DownloadUrl
            : _manifest.ReleaseUrl;
    }

    private static bool OpenUrl(string? url)
    {
        if (!IsValidHttpsUrl(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            System.Windows.MessageBox.Show(
                "The update link is missing or invalid.",
                "Update",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = uri.ToString(),
            UseShellExecute = true
        });
        return true;
    }

    private static bool IsValidHttpsUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;
    }
}
