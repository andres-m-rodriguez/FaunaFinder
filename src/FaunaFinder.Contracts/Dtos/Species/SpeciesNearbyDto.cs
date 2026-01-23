namespace FaunaFinder.Contracts.Dtos.Species;

/// <summary>
/// Represents a species found within a search radius, including its nearest location.
/// </summary>
public sealed record SpeciesNearbyDto(
    int Id,
    string CommonName,
    string ScientificName,
    double DistanceMeters,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string? LocationDescription,
    List<SpeciesTranslationDto> Translations
);
