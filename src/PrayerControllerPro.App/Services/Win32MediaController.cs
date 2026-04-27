using System.Runtime.InteropServices;

namespace PrayerControllerPro.App.Services;

public sealed class Win32MediaController
{
    private const int VkMediaPlayPause = 0xB3;
    private const int KeyEventFExtendedKey = 0x0001;
    private const int KeyEventFKeyUp = 0x0002;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, int extraInfo);

    public void TogglePlayPause()
    {
        keybd_event((byte)VkMediaPlayPause, 0, KeyEventFExtendedKey, 0);
        keybd_event((byte)VkMediaPlayPause, 0, KeyEventFExtendedKey | KeyEventFKeyUp, 0);
    }
}
