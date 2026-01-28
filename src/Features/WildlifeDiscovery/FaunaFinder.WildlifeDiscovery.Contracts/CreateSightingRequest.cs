namespace FaunaFinder.WildlifeDiscovery.Contracts;

public record CreateSightingRequest(
    int SpeciesId,
    double Latitude,
    double Longitude,
    DateTime ObservedAt,
    string Mode,
    string Confidence,
    string Count,
    int Behaviors,
    int Evidence,
    string? Weather,
    string? Notes,
    byte[]? PhotoData,
    string? PhotoContentType);
