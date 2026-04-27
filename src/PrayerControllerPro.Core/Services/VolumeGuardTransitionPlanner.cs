using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Services;

public static class VolumeGuardTransitionPlanner
{
    public static IReadOnlyList<double> CreateSteps(double startVolume, double targetVolume, VolumeGuardTransitionMode mode)
    {
        var start = Math.Clamp(startVolume, 0d, 1d);
        var target = Math.Clamp(targetVolume, 0d, 1d);
        var stepCount = GetStepCount(mode);
        var steps = new List<double>(stepCount);

        for (var index = 1; index <= stepCount; index++)
        {
            var progress = index / (double)stepCount;
            if (mode is VolumeGuardTransitionMode.Smooth or VolumeGuardTransitionMode.Smoother)
            {
                progress = SmoothStep(progress);
            }

            steps.Add(start + ((target - start) * progress));
        }

        steps[^1] = target;
        return steps;
    }

    public static TimeSpan GetStepDelay(VolumeGuardTransitionMode mode)
    {
        return mode switch
        {
            VolumeGuardTransitionMode.Fast => TimeSpan.FromMilliseconds(120),
            VolumeGuardTransitionMode.Slow => TimeSpan.FromMilliseconds(150),
            VolumeGuardTransitionMode.Smooth => TimeSpan.FromMilliseconds(110),
            VolumeGuardTransitionMode.Smoother => TimeSpan.FromMilliseconds(100),
            _ => TimeSpan.FromMilliseconds(120)
        };
    }

    private static int GetStepCount(VolumeGuardTransitionMode mode)
    {
        return mode switch
        {
            VolumeGuardTransitionMode.Fast => 2,
            VolumeGuardTransitionMode.Slow => 4,
            VolumeGuardTransitionMode.Smooth => 8,
            VolumeGuardTransitionMode.Smoother => 12,
            _ => 2
        };
    }

    private static double SmoothStep(double progress)
    {
        return progress * progress * (3d - (2d * progress));
    }
}
