namespace FaunaFinder.Wildlife.Contracts.Dtos;

/// <summary>
/// Overview statistics for the public dashboard.
/// </summary>
public sealed record StatisticsOverviewDto(
    int TotalApprovedSightings,
    int TotalSpecies,
    int TotalMunicipalities,
    int SightingsThisMonth
);

/// <summary>
/// Sightings count grouped by month for time series chart.
/// </summary>
public sealed record SightingsByMonthDto(
    int Year,
    int Month,
    int Count
);

/// <summary>
/// Species count grouped by category (fauna/flora).
/// </summary>
public sealed record SpeciesByCategoryDto(
    string Category,
    int Count
);

/// <summary>
/// Species/sightings count grouped by municipality.
/// </summary>
public sealed record SightingsByMunicipalityDto(
    int MunicipalityId,
    string MunicipalityName,
    int SightingsCount
);

/// <summary>
/// Top observed species with sighting counts.
/// </summary>
public sealed record TopSpeciesDto(
    int SpeciesId,
    string SpeciesName,
    int SightingsCount
);

/// <summary>
/// Complete statistics response for the public dashboard.
/// </summary>
public sealed record PublicStatisticsDto(
    StatisticsOverviewDto Overview,
    IReadOnlyList<SightingsByMonthDto> SightingsByMonth,
    IReadOnlyList<SpeciesByCategoryDto> SpeciesByCategory,
    IReadOnlyList<SightingsByMunicipalityDto> SightingsByMunicipality,
    IReadOnlyList<TopSpeciesDto> TopSpecies
);
