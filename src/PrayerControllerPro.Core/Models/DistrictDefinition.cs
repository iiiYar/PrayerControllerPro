namespace PrayerControllerPro.Core.Models;

public sealed class DistrictDefinition
{
    public string Id { get; init; } = string.Empty;

    public string CityId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }
}
