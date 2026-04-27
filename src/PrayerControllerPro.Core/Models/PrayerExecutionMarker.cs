namespace PrayerControllerPro.Core.Models;

public sealed class PrayerExecutionMarker
{
    public bool PauseTriggered { get; set; }

    public bool AdhanTriggered { get; set; }

    public bool IqamaTriggered { get; set; }

    public bool ResumeTriggered { get; set; }

    public bool WasPausedByApp { get; set; }
}
