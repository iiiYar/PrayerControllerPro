namespace PrayerControllerPro.Core.Models;

public sealed class PrayerExecutionState
{
    public DateOnly Date { get; set; }

    public Dictionary<string, PrayerExecutionMarker> Markers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public PrayerExecutionMarker GetOrCreate(string prayerId)
    {
        if (!Markers.TryGetValue(prayerId, out var marker))
        {
            marker = new PrayerExecutionMarker();
            Markers[prayerId] = marker;
        }

        return marker;
    }

    public void Reset(DateOnly date)
    {
        Date = date;
        Markers.Clear();
    }
}
