using System.Diagnostics;
using NAudio.CoreAudioApi;
using PrayerControllerPro.App.Services.Logging;
using PrayerControllerPro.Core.Models;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.App.Services.System;

public sealed class VolumeGuardService(AppLogService logService)
{
    private readonly Dictionary<string, GuardedSession> _guardedSessions = new(StringComparer.OrdinalIgnoreCase);

    public bool IsActive => _guardedSessions.Count > 0;

    public int Protect(double targetVolume, VolumeGuardTransitionMode transitionMode)
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
                        StartTransition(guardedSession, guardedVolume, transitionMode);
                        protectedCount++;
                    }
                }
                catch (Exception ex)
                {
                    logService.Warning("System", "Skipped audio session while applying volume guard.", ex.Message);
                }
            }

            return protectedCount;
        }
        catch (Exception ex)
        {
            logService.Warning("System", "Volume guard protection failed.", ex.Message);
            return 0;
        }
    }

    public int Restore(VolumeGuardTransitionMode transitionMode)
    {
        var restoredCount = 0;
        var guardedSessions = _guardedSessions.Values.ToList();
        _guardedSessions.Clear();

        foreach (var guardedSession in guardedSessions)
        {
            try
            {
                StartTransition(guardedSession, guardedSession.OriginalVolume, transitionMode);
                restoredCount++;
            }
            catch (Exception ex)
            {
                logService.Warning("System", "Skipped audio session while restoring volume guard.", ex.Message);
            }
        }

        return restoredCount;
    }

    private void StartTransition(
        GuardedSession guardedSession,
        float targetVolume,
        VolumeGuardTransitionMode transitionMode)
    {
        if (guardedSession.IsTransitionActive
            && Math.Abs(guardedSession.TargetVolume - targetVolume) < 0.001f
            && guardedSession.TransitionMode == transitionMode)
        {
            return;
        }

        guardedSession.TransitionCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        guardedSession.TransitionCancellation = cancellation;
        guardedSession.TargetVolume = targetVolume;
        guardedSession.TransitionMode = transitionMode;

        _ = RunTransitionAsync(guardedSession, targetVolume, transitionMode, cancellation);
    }

    private async Task RunTransitionAsync(
        GuardedSession guardedSession,
        float targetVolume,
        VolumeGuardTransitionMode transitionMode,
        CancellationTokenSource cancellation)
    {
        try
        {
            var steps = VolumeGuardTransitionPlanner.CreateSteps(
                guardedSession.Volume.Volume,
                targetVolume,
                transitionMode);
            var delay = VolumeGuardTransitionPlanner.GetStepDelay(transitionMode);

            for (var index = 0; index < steps.Count; index++)
            {
                cancellation.Token.ThrowIfCancellationRequested();
                guardedSession.Volume.Volume = (float)steps[index];

                if (index < steps.Count - 1)
                {
                    await Task.Delay(delay, cancellation.Token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine($"[Warning] Volume guard transition cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            logService.Warning("System", "Volume guard transition stopped before completion.", ex.Message);
        }
        finally
        {
            if (ReferenceEquals(guardedSession.TransitionCancellation, cancellation))
            {
                guardedSession.TransitionCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    private string ResolveSessionKey(AudioSessionControl session, int processId, int index)
    {
        try
        {
            return session.GetSessionInstanceIdentifier;
        }
        catch (Exception ex)
        {
            logService.Warning("System", "Audio session identifier was unavailable.", ex.Message);
            return $"{processId}:{index}";
        }
    }

    private sealed class GuardedSession(SimpleAudioVolume volume, float originalVolume)
    {
        public SimpleAudioVolume Volume { get; } = volume;
        public float OriginalVolume { get; } = originalVolume;
        public float TargetVolume { get; set; } = originalVolume;
        public VolumeGuardTransitionMode TransitionMode { get; set; } = VolumeGuardTransitionMode.Fast;
        public CancellationTokenSource? TransitionCancellation { get; set; }
        public bool IsTransitionActive => TransitionCancellation is { IsCancellationRequested: false };
    }
}
