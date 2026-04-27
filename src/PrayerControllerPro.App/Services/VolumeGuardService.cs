using NAudio.CoreAudioApi;

namespace PrayerControllerPro.App.Services;

public sealed class VolumeGuardService
{
    private readonly Dictionary<string, GuardedSession> _guardedSessions = new(StringComparer.OrdinalIgnoreCase);

    public bool IsActive => _guardedSessions.Count > 0;

    public int Protect(double targetVolume)
    {
        try
        {
            var protectedCount = 0;
            var currentProcessId = Environment.ProcessId;
            var target = (float)Math.Clamp(targetVolume, 0d, 1d);

            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            for (var index = 0; index < sessions.Count; index++)
            {
                try
                {
                    var session = sessions[index];
                    var processId = (int)session.GetProcessID;

                    if (processId == 0 || processId == currentProcessId || session.IsSystemSoundsSession)
                    {
                        continue;
                    }

                    var sessionKey = ResolveSessionKey(session, processId, index);
                    var volume = session.SimpleAudioVolume;
                    var originalVolume = volume.Volume;

                    if (!_guardedSessions.TryGetValue(sessionKey, out var guardedSession))
                    {
                        guardedSession = new GuardedSession(volume, originalVolume);
                        _guardedSessions[sessionKey] = guardedSession;
                    }

                    var guardedVolume = Math.Min(guardedSession.OriginalVolume, target);
                    if (volume.Volume > guardedVolume + 0.001f)
                    {
                        volume.Volume = guardedVolume;
                        protectedCount++;
                    }
                }
                catch
                {
                    // Players can create or destroy audio sessions while we enumerate them.
                }
            }

            return protectedCount;
        }
        catch
        {
            return 0;
        }
    }

    public int Restore()
    {
        var restoredCount = 0;

        foreach (var guardedSession in _guardedSessions.Values)
        {
            try
            {
                guardedSession.Volume.Volume = guardedSession.OriginalVolume;
                restoredCount++;
            }
            catch
            {
                // Audio sessions can disappear when a player/browser tab closes.
            }
        }

        _guardedSessions.Clear();
        return restoredCount;
    }

    private static string ResolveSessionKey(AudioSessionControl session, int processId, int index)
    {
        try
        {
            return session.GetSessionInstanceIdentifier;
        }
        catch
        {
            return $"{processId}:{index}";
        }
    }

    private sealed record GuardedSession(SimpleAudioVolume Volume, float OriginalVolume);
}
