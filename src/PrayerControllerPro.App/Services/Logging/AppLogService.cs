using System.Globalization;
using System.IO;
using System.Text.Json;

namespace PrayerControllerPro.App.Services.Logging;

public sealed class AppLogService(string logDirectory)
{
    private readonly object _syncRoot = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string LogDirectory { get; } = logDirectory;

    public void Info(string area, string message, string? details = null)
    {
        Write("Info", area, message, details);
    }

    public void Warning(string area, string message, string? details = null)
    {
        Write("Warning", area, message, details);
    }

    public void Error(string area, string message, Exception exception)
    {
        Write("Error", area, message, exception.ToString());
    }

    public IReadOnlyList<AppLogEntry> ReadRecent(int maxEntries = 800)
    {
        if (!Directory.Exists(LogDirectory))
        {
            return [];
        }

        var entries = new List<AppLogEntry>();
        foreach (var file in Directory.EnumerateFiles(LogDirectory, "*.jsonl").OrderByDescending(Path.GetFileName).Take(14))
        {
            foreach (var line in File.ReadLines(file))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    var entry = JsonSerializer.Deserialize<AppLogEntry>(line, _jsonOptions);
                    if (entry is not null)
                    {
                        entries.Add(entry);
                    }
                }
                catch (JsonException)
                {
                    entries.Add(new AppLogEntry
                    {
                        Timestamp = File.GetLastWriteTime(file),
                        Level = "Warning",
                        Area = "Logs",
                        Message = "Skipped malformed log line.",
                        Details = line
                    });
                }
            }
        }

        return entries
            .OrderByDescending(entry => entry.Timestamp)
            .Take(maxEntries)
            .ToList();
    }

    public void ClearAll()
    {
        if (!Directory.Exists(LogDirectory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(LogDirectory, "*.jsonl"))
        {
            File.Delete(file);
        }
    }

    private void Write(string level, string area, string message, string? details)
    {
        var entry = new AppLogEntry
        {
            Timestamp = DateTimeOffset.Now,
            Level = level,
            Area = area,
            Message = message,
            Details = details
        };

        lock (_syncRoot)
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(GetTodayLogPath(), JsonSerializer.Serialize(entry, _jsonOptions) + Environment.NewLine);
        }
    }

    private string GetTodayLogPath()
    {
        var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.jsonl";
        return Path.Combine(LogDirectory, fileName);
    }
}
