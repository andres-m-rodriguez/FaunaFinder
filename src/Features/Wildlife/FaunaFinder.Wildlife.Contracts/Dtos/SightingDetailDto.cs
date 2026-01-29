namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SightingDetailDto(
    int Id,
    int SpeciesId,
    string? SpeciesName,
    string? SpeciesScientificName,
    string Mode,
    string Confidence,
    string Count,
    int Behaviors,
    int Evidence,
    string? Weather,
    string? Notes,
    double Latitude,
    double Longitude,
    int? MunicipalityId,
    DateTime ObservedAt,
    DateTime CreatedAt,
    bool HasPhoto,
    string Status,
    bool IsFlaggedForReview,
    bool IsNewMunicipalityRecord,
    string? ReviewNotes,
    DateTime? ReviewedAt,
    int? ReviewedByUserId,
    int ReportedByUserId
);
