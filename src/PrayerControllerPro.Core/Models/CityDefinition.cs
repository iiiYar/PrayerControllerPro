namespace PrayerControllerPro.Core.Models;

public sealed class CityDefinition
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string ApiCity { get; init; } = string.Empty;

    public string ApiCountry { get; init; } = string.Empty;

    public string TimeZoneId { get; init; } = string.Empty;

    public int DefaultMethod { get; init; }
}
