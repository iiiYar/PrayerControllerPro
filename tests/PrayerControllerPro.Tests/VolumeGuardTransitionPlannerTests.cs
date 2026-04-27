using PrayerControllerPro.Core.Models;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.Tests;

public sealed class VolumeGuardTransitionPlannerTests
{
    [Fact]
    public void CreateSteps_FastUsesTwoLinearStages()
    {
        var steps = VolumeGuardTransitionPlanner.CreateSteps(1d, 0d, VolumeGuardTransitionMode.Fast);

        Assert.Collection(
            steps,
            step => Assert.Equal(0.5d, step, precision: 3),
            step => Assert.Equal(0d, step));
    }

    [Theory]
    [InlineData(VolumeGuardTransitionMode.Fast, 2)]
    [InlineData(VolumeGuardTransitionMode.Slow, 4)]
    [InlineData(VolumeGuardTransitionMode.Smooth, 8)]
    [InlineData(VolumeGuardTransitionMode.Smoother, 12)]
    public void CreateSteps_UsesMoreStagesForSmootherModes(VolumeGuardTransitionMode mode, int expectedCount)
    {
        var steps = VolumeGuardTransitionPlanner.CreateSteps(1d, 0d, mode);

        Assert.Equal(expectedCount, steps.Count);
        Assert.Equal(0d, steps[^1]);
    }

    [Fact]
    public void CreateSteps_CanRestoreBackToOriginalVolume()
    {
        var steps = VolumeGuardTransitionPlanner.CreateSteps(0d, 1d, VolumeGuardTransitionMode.Slow);

        Assert.Equal(0.25d, steps[0], precision: 3);
        Assert.Equal(1d, steps[^1]);
    }
}
