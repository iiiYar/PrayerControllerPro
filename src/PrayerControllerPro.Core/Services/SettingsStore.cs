using System.Globalization;
using System.Text.Json;
using PrayerControllerPro.Core.Catalogs;
using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Services;

public sealed class SettingsStore(string settingsFilePath, LegacySettingsMigrator migrator)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task<AppSettings> LoadAsync(IEnumerable<string> legacyDirectories, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath)!);

        if (File.Exists(settingsFilePath))
        {
            try
            {
                await using var stream = File.OpenRead(settingsFilePath);
                var loaded = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false)
                    ?? AppCatalog.CreateDefaultSettings();
                AppCatalog.EnsureDefaults(loaded);
                return loaded;
            }
            catch (JsonException)
            {
                BackupInvalidSettingsFile();
                var fallbackSettings = AppCatalog.CreateDefaultSettings();
                await SaveAsync(fallbackSettings, cancellationToken).ConfigureAwait(false);
                return fallbackSettings;
            }
        }

        var migrated = migrator.TryMigrate(legacyDirectories);
        if (migrated is not null)
        {
            await SaveAsync(migrated, cancellationToken).ConfigureAwait(false);
            return migrated;
        }

        var defaults = AppCatalog.CreateDefaultSettings();
        await SaveAsync(defaults, cancellationToken).ConfigureAwait(false);
        return defaults;
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        AppCatalog.EnsureDefaults(settings);
        Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath)!);

        await using var stream = File.Create(settingsFilePath);
        await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private void BackupInvalidSettingsFile()
    {
        var backupPath = $"{settingsFilePath}.invalid-{DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)}";

        try
        {
            File.Move(settingsFilePath, backupPath);
        }
        catch
        {
            // If the backup fails, loading defaults is still safer than crashing startup.
        }
    }
}
