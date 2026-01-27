namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed record CreateUserSpeciesRequest(
    string CommonName,
    string? ScientificName,
    string? Description,
    string? PhotoBase64,
    string? PhotoContentType);
