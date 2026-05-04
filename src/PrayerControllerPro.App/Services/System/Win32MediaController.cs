using System.ComponentModel;
using System.Runtime.InteropServices;
using PrayerControllerPro.App.Services.Logging;

namespace PrayerControllerPro.App.Services.System;

// Media control is sent as a global Windows media-key event.
// Works well for Windows Media Player, Spotify, and most native audio apps.
// Browser-based media may ignore the event depending on browser focus and site behavior.
// This service cannot target a specific process; it toggles the system Play/Pause key.
public sealed class Win32MediaController(AppLogService logService)
{
    private const byte VkMediaPlayPause = 0xB3;
    private const uint InputKeyboard = 1;
    private const uint KeyEventFExtendedKey = 0x0001;
    private const uint KeyEventFKeyUp = 0x0002;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, int extraInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);

    public void TogglePlayPause()
    {
        try
        {
            SendPlayPauseWithKeybdEvent();
            logService.Info("Media", "Sent Play/Pause media key.", "Method=keybd_event");
        }
        catch (Exception ex)
        {
            logService.Warning("Media", "keybd_event media control failed; trying SendInput fallback.", ex.Message);
            SendPlayPauseWithSendInput();
        }
    }

    private static void SendPlayPauseWithKeybdEvent()
    {
        keybd_event(VkMediaPlayPause, 0, KeyEventFExtendedKey, 0);
        keybd_event(VkMediaPlayPause, 0, KeyEventFExtendedKey | KeyEventFKeyUp, 0);
    }

    private void SendPlayPauseWithSendInput()
    {
        Input[] inputs =
        [
            CreateKeyboardInput(flags: 0),
            CreateKeyboardInput(KeyEventFKeyUp)
        ];

        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        if (sent == inputs.Length)
        {
            logService.Info("Media", "Sent Play/Pause media key.", "Method=SendInput");
            return;
        }

        var error = Marshal.GetLastWin32Error();
        logService.Warning("Media", "SendInput media control failed.", new Win32Exception(error).Message);
    }

    private static Input CreateKeyboardInput(uint flags)
    {
        return new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = VkMediaPlayPause,
                    Flags = flags
                }
            }
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KeyboardInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }
}
