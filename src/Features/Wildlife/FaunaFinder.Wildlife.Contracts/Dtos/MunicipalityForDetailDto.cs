namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record MunicipalityForDetailDto(
    int Id,
    string Name,
    string GeoJsonId,
    int SpeciesCount
);
