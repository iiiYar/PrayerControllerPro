using PrayerControllerPro.Core.Models;
using PrayerControllerPro.Core.Services;

namespace PrayerControllerPro.Tests;

public class SchedulerEngineTests
{
    private readonly SchedulerEngine _scheduler = new();

    [Fact]
    public void Evaluate_Pauses_WhenAppEntersPrayerWindowLate()
    {
        var prayerTime = new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.FromHours(3));
        var schedule = CreateSchedule(
            prayerTime,
            new PrayerRuleSettings
            {
                Enabled = true,
                StopBeforeMinutes = 5,
                ResumeAfterMinutes = 15
            });

        var actions = _scheduler.Evaluate(
            prayerTime.AddMinutes(-2),
            schedule,
            new PrayerExecutionState());

        Assert.Single(actions);
        Assert.Equal(SchedulerActionKind.PauseMedia, actions[0].Kind);
    }

    [Fact]
    public void Evaluate_DoesNotReplayPastPrayerAfterWindowEnds()
    {
        var prayerTime = new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.FromHours(3));
        var schedule = CreateSchedule(
            prayerTime,
            new PrayerRuleSettings
            {
                Enabled = true,
                StopBeforeMinutes = 5,
                ResumeAfterMinutes = 15
            });

        var actions = _scheduler.Evaluate(
            prayerTime.AddMinutes(45),
            schedule,
            new PrayerExecutionState());

        Assert.Empty(actions);
    }

    [Fact]
    public void Evaluate_ResumesOnce_AfterMediaWasPausedByApp()
    {
        var prayerTime = new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.FromHours(3));
        var schedule = CreateSchedule(
            prayerTime,
            new PrayerRuleSettings
            {
                Enabled = true,
                StopBeforeMinutes = 5,
                ResumeAfterMinutes = 15
            });
        var state = new PrayerExecutionState();

        _scheduler.Evaluate(prayerTime.AddMinutes(-2), schedule, state);
        var actions = _scheduler.Evaluate(prayerTime.AddMinutes(41), schedule, state);

        Assert.Single(actions);
        Assert.Equal(SchedulerActionKind.ResumeMedia, actions[0].Kind);

        var secondPass = _scheduler.Evaluate(prayerTime.AddMinutes(42), schedule, state);
        Assert.Empty(secondPass);
    }

    [Fact]
    public void Evaluate_DoesNotResume_BeforeIqamaPlusResumeDelay()
    {
        var prayerTime = new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.FromHours(3));
        var schedule = CreateSchedule(
            prayerTime,
            new PrayerRuleSettings
            {
                Enabled = true,
                StopBeforeMinutes = 10,
                ResumeAfterMinutes = 10
            });
        var state = new PrayerExecutionState();

        _scheduler.Evaluate(prayerTime.AddMinutes(-10), schedule, state);
        var beforeResume = _scheduler.Evaluate(prayerTime.AddMinutes(30), schedule, state);
        var atResume = _scheduler.Evaluate(prayerTime.AddMinutes(35), schedule, state);

        Assert.Empty(beforeResume);
        Assert.Single(atResume);
        Assert.Equal(SchedulerActionKind.ResumeMedia, atResume[0].Kind);
    }

    [Fact]
    public void Evaluate_DoesNotPauseAgain_WhenExecutionStateIsPreserved()
    {
        var prayerTime = new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.FromHours(3));
        var schedule = CreateSchedule(
            prayerTime,
            new PrayerRuleSettings
            {
                Enabled = true,
                StopBeforeMinutes = 5,
                ResumeAfterMinutes = 15
            });
        var state = new PrayerExecutionState();

        _scheduler.Evaluate(prayerTime.AddMinutes(-2), schedule, state);
        var actionsAfterRefresh = _scheduler.Evaluate(prayerTime.AddMinutes(1), schedule, state);

        Assert.Empty(actionsAfterRefresh);
    }

    [Fact]
    public void GetCountdown_PrefersResumeWhilePrayerWindowIsActive()
    {
        var prayerTime = new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.FromHours(3));
        var schedule = CreateSchedule(
            prayerTime,
            new PrayerRuleSettings
            {
                Enabled = true,
                StopBeforeMinutes = 5,
                ResumeAfterMinutes = 15
            });

        var countdown = _scheduler.GetCountdown(prayerTime.AddMinutes(1), schedule);

        Assert.Equal(CountdownMode.ResumeAfterPrayer, countdown.Mode);
        Assert.Equal("Fajr", countdown.PrayerId);
        Assert.Equal(prayerTime.AddMinutes(40), countdown.TargetTime);
    }

    private static DailyPrayerSchedule CreateSchedule(DateTimeOffset prayerTime, PrayerRuleSettings rule)
    {
        return new DailyPrayerSchedule
        {
            Date = DateOnly.FromDateTime(prayerTime.Date),
            CityId = "riyadh",
            CalculationMethod = 4,
            Source = "Test",
            Entries =
            [
                new PrayerScheduleEntry
                {
                    Id = "Fajr",
                    DisplayName = "الفجر",
                    IconGlyph = "🌅",
                    IsCustom = false,
                    PrayerTime = prayerTime,
                    IqamaTime = prayerTime.AddMinutes(25),
                    Rule = rule
                }
            ]
        };
    }
}
