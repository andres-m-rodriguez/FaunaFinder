namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed record CreateSightingRequest(
    int? SpeciesId,
    int? UserSpeciesId,
    string Mode,
    string Confidence,
    string Count,
    int Behaviors,
    int Evidence,
    string? Weather,
    string? Notes,
    double Latitude,
    double Longitude,
    DateTime ObservedAt,
    string? PhotoBase64,
    string? PhotoContentType,
    string? AudioBase64,
    string? AudioContentType);
