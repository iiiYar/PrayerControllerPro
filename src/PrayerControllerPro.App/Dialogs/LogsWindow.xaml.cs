using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using PrayerControllerPro.App.Services;

namespace PrayerControllerPro.App.Dialogs;

public partial class LogsWindow : Window
{
    private readonly AppLogService _logService;

    public LogsWindow(AppLogService logService)
    {
        InitializeComponent();
        _logService = logService;

        LogFolderTextBlock.Text = _logService.LogDirectory;
        RefreshButton.Click += (_, _) => RefreshLogs();
        OpenFolderButton.Click += (_, _) => OpenLogFolder();
        ClearButton.Click += (_, _) => ClearLogs();

        RefreshLogs();
    }

    private void RefreshLogs()
    {
        var builder = new StringBuilder();
        foreach (var entry in _logService.ReadRecent())
        {
            builder.Append(entry.Timestamp.ToString("MM-dd HH:mm:ss", CultureInfo.InvariantCulture).PadRight(14));
            builder.Append(entry.Level.PadRight(9));
            builder.Append(entry.Area.PadRight(13));
            builder.Append(entry.Message);

            if (!string.IsNullOrWhiteSpace(entry.Details))
            {
                builder.Append(" | ");
                builder.Append(entry.Details.Replace(Environment.NewLine, " "));
            }

            builder.AppendLine();
        }

        LogTextBox.Text = builder.Length == 0 ? "No logs yet." : builder.ToString();
    }

    private void OpenLogFolder()
    {
        Directory.CreateDirectory(_logService.LogDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = _logService.LogDirectory,
            UseShellExecute = true
        });
    }

    private void ClearLogs()
    {
        var result = System.Windows.MessageBox.Show(
            this,
            "Clear all log files?",
            "Clear logs",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _logService.ClearAll();
        _logService.Info("Logs", "Logs were cleared from the viewer.");
        RefreshLogs();
    }
}
