using Microsoft.Win32;

namespace PrayerControllerPro.App.Services;

public sealed class AutoStartService
{
    private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "PrayerControllerPro";

    public void Apply(bool enabled, string executablePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: true);
        if (key is null)
        {
            return;
        }

        if (enabled)
        {
            key.SetValue(AppName, $"\"{executablePath}\"");
            return;
        }

        key.DeleteValue(AppName, false);
    }
}
