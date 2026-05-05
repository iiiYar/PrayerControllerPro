using System.IO;
using System.Windows.Media;

namespace PrayerControllerPro.App.Services.Audio;

public sealed class AudioPlaybackService : IDisposable
{
    private readonly MediaPlayer _player = new();
    private bool _pendingPlay;

    public AudioPlaybackService()
    {
        _player.MediaOpened += (_, _) =>
        {
            if (_pendingPlay)
            {
                _player.Position = TimeSpan.Zero;
                _player.Play();
                _pendingPlay = false;
            }
        };

        _player.MediaFailed += (_, _) => _pendingPlay = false;
    }

    public bool CanPlay(string? filePath)
    {
        return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
    }

    public void Play(string? filePath, double volume)
    {
        if (!CanPlay(filePath))
        {
            return;
        }

        _pendingPlay = true;
        _player.Volume = Math.Clamp(volume, 0d, 1d);
        _player.Open(new Uri(filePath!, UriKind.Absolute));
    }

    public void Stop()
    {
        _pendingPlay = false;
        _player.Stop();
    }

    public void Dispose()
    {
        _player.Close();
    }
}
