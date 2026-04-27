namespace PrayerControllerPro.Core.Models;

public sealed class AudioSettings
{
    public bool EnableAdhanAudio { get; set; }

    public bool EnableIqamaAudio { get; set; }

    public string? AdhanAudioPath { get; set; }

    public string? IqamaAudioPath { get; set; }

    public string? AdhanPresetUrl { get; set; }

    public string? IqamaPresetUrl { get; set; }

    public double Volume { get; set; } = 0.8d;

    public MediaControlMode MediaControlMode { get; set; } = MediaControlMode.PlayPauseKey;

    public double VolumeGuardLevel { get; set; }
}
