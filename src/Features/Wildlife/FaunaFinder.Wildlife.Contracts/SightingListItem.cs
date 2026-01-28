namespace FaunaFinder.Wildlife.Contracts;

public record SightingListItem(
    int Id,
    int SpeciesId,
    string? SpeciesName,
    string Mode,
    string Confidence,
    string Count,
    string Status,
    DateTime ObservedAt,
    DateTime CreatedAt,
    double Latitude,
    double Longitude,
    bool HasPhoto);
