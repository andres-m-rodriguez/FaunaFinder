namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesSightingCountDto(
    int SpeciesId,
    string CommonName,
    string ScientificName,
    int ApprovedSightingCount);
