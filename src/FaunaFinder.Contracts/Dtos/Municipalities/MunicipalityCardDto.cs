namespace FaunaFinder.Contracts.Dtos.Municipalities;

public sealed record MunicipalityCardDto(
    int Id,
    string Name,
    int SpeciesCount
);
