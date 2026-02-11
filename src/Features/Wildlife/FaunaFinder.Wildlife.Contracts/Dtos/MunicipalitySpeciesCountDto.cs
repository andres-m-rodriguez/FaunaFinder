namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record MunicipalitySpeciesCountDto(
    int MunicipalityId,
    string MunicipalityName,
    int SpeciesCount);
