namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed record ReviewSightingRequest(
    string Status,
    string? ReviewNotes);
