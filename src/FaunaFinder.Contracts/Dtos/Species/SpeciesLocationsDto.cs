using FaunaFinder.Contracts.Localization;

namespace FaunaFinder.Contracts.Dtos.Species;

/// <summary>
/// Represents a species with all of its locations.
/// Used for batch retrieval of species locations.
/// </summary>
public sealed record SpeciesLocationsDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    List<SpeciesLocationDto> Locations
);
