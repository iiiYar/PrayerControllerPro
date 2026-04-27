namespace PrayerControllerPro.Core.Models;

public sealed class PrayerRuleSettings
{
    public bool Enabled { get; set; } = true;

    public int StopBeforeMinutes { get; set; } = 5;

    public int ResumeAfterMinutes { get; set; } = 15;

    public bool PlayAdhan { get; set; }

    public bool PlayIqama { get; set; }

    public int? IqamaOffsetMinutes { get; set; }

    public PrayerRuleSettings Clone()
    {
        return new PrayerRuleSettings
        {
            Enabled = Enabled,
            StopBeforeMinutes = StopBeforeMinutes,
            ResumeAfterMinutes = ResumeAfterMinutes,
            PlayAdhan = PlayAdhan,
            PlayIqama = PlayIqama,
            IqamaOffsetMinutes = IqamaOffsetMinutes
        };
    }
}
