namespace PrayerControllerPro.Core.Models;

public sealed class PrayerDefinition
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string IconGlyph { get; init; } = string.Empty;

    public int DefaultIqamaOffsetMinutes { get; init; }

    public bool IsCustom { get; init; }
}
