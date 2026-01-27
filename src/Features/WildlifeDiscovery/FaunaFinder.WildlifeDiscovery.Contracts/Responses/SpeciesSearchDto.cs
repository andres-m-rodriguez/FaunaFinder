namespace FaunaFinder.WildlifeDiscovery.Contracts.Responses;

public sealed record SpeciesSearchDto(
    int Id,
    string CommonName,
    string ScientificName,
    bool IsEndangered);
