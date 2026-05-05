using PrayerControllerPro.Core.Models;

namespace PrayerControllerPro.Core.Services;

public sealed class SchedulerEngine
{
    private readonly TimeSpan _audioCatchUpWindow = TimeSpan.FromMinutes(5);

    public IReadOnlyList<SchedulerAction> Evaluate(DateTimeOffset now, DailyPrayerSchedule schedule, PrayerExecutionState state)
    {
        if (state.Date != schedule.Date)
        {
            state.Reset(schedule.Date);
        }

        var actions = new List<SchedulerAction>();

        foreach (var entry in schedule.Entries.Where(entry => entry.Rule.Enabled))
        {
            var marker = state.GetOrCreate(entry.Id);
            var stopTime = entry.PrayerTime.AddMinutes(-entry.Rule.StopBeforeMinutes);
            var resumeTime = entry.IqamaTime.AddMinutes(entry.Rule.ResumeAfterMinutes);

            if (!marker.PauseTriggered && now >= stopTime && now < resumeTime)
            {
                marker.PauseTriggered = true;
                marker.WasPausedByApp = true;
                actions.Add(new SchedulerAction
                {
                    Kind = SchedulerActionKind.PauseMedia,
                    PrayerId = entry.Id,
                    PrayerName = entry.DisplayName,
                    Message = $"Media paused automatically for {entry.DisplayName}."
                });
            }

            if (entry.Rule.PlayAdhan && !marker.AdhanTriggered && now >= entry.PrayerTime && now <= entry.PrayerTime + _audioCatchUpWindow)
            {
                marker.AdhanTriggered = true;
                actions.Add(new SchedulerAction
                {
                    Kind = SchedulerActionKind.PlayAdhan,
                    PrayerId = entry.Id,
                    PrayerName = entry.DisplayName,
                    Message = $"Playing adhan for {entry.DisplayName}."
                });
            }

            if (entry.Rule.PlayIqama && !marker.IqamaTriggered && now >= entry.IqamaTime && now <= entry.IqamaTime + _audioCatchUpWindow)
            {
                marker.IqamaTriggered = true;
                actions.Add(new SchedulerAction
                {
                    Kind = SchedulerActionKind.PlayIqama,
                    PrayerId = entry.Id,
                    PrayerName = entry.DisplayName,
                    Message = $"Playing iqama for {entry.DisplayName}."
                });
            }

            if (marker.WasPausedByApp && !marker.ResumeTriggered && now >= resumeTime)
            {
                marker.ResumeTriggered = true;
                marker.WasPausedByApp = false;
                actions.Add(new SchedulerAction
                {
                    Kind = SchedulerActionKind.ResumeMedia,
                    PrayerId = entry.Id,
                    PrayerName = entry.DisplayName,
                    Message = $"Media resumed after {entry.DisplayName}."
                });
            }
        }

        return actions;
    }

    public CountdownInfo GetCountdown(DateTimeOffset now, DailyPrayerSchedule schedule)
    {
        var active = schedule.Entries
            .Where(entry => entry.Rule.Enabled)
            .Select(entry => new
            {
                Entry = entry,
                StopTime = entry.PrayerTime.AddMinutes(-entry.Rule.StopBeforeMinutes),
                ResumeTime = entry.IqamaTime.AddMinutes(entry.Rule.ResumeAfterMinutes)
            })
            .FirstOrDefault(item => now >= item.StopTime && now < item.ResumeTime);

        if (active is not null)
        {
            return new CountdownInfo
            {
                Mode = CountdownMode.ResumeAfterPrayer,
                PrayerId = active.Entry.Id,
                PrayerName = active.Entry.DisplayName,
                TargetTime = active.ResumeTime,
                Remaining = active.ResumeTime - now
            };
        }

        var next = schedule.Entries.FirstOrDefault(entry => entry.PrayerTime > now);
        if (next is null)
        {
            var first = schedule.Entries.First();
            var nextDayTarget = first.PrayerTime.AddDays(1);
            return new CountdownInfo
            {
                Mode = CountdownMode.NextPrayer,
                PrayerId = first.Id,
                PrayerName = first.DisplayName,
                TargetTime = nextDayTarget,
                Remaining = nextDayTarget - now
            };
        }

        return new CountdownInfo
        {
            Mode = CountdownMode.NextPrayer,
            PrayerId = next.Id,
            PrayerName = next.DisplayName,
            TargetTime = next.PrayerTime,
            Remaining = next.PrayerTime - now
        };
    }
}
