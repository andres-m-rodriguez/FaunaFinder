namespace FaunaFinder.Contracts.Dtos.Municipalities;

public sealed record MunicipalityForDetailDto(
    int Id,
    string Name,
    string GeoJsonId,
    int SpeciesCount
);
