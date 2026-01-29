namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record MunicipalityCardDto(
    int Id,
    string Name,
    int SpeciesCount
);
