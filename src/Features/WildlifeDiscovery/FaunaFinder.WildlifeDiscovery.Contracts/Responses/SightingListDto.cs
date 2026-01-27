namespace FaunaFinder.WildlifeDiscovery.Contracts.Responses;

public sealed record SightingListDto(
    int Id,
    string SpeciesName,
    string Mode,
    string Confidence,
    string Count,
    double Latitude,
    double Longitude,
    string? MunicipalityName,
    DateTime ObservedAt,
    string Status,
    bool IsFlaggedForReview,
    bool IsNewMunicipalityRecord,
    string ReportedByUserName);
