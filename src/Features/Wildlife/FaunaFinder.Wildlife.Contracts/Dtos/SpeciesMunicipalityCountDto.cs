namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesMunicipalityCountDto(
    int SpeciesId,
    string CommonName,
    string ScientificName,
    int MunicipalityCount);
