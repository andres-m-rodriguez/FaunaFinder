namespace FaunaFinder.WildlifeDiscovery.Contracts.Responses;

public sealed record UserSpeciesDto(
    int Id,
    string CommonName,
    string? ScientificName,
    string? Description,
    bool HasPhoto,
    bool IsVerified,
    bool IsEndangered,
    int CreatedByUserId,
    string CreatedByUserName,
    DateTime CreatedAt,
    DateTime? VerifiedAt,
    int? VerifiedByUserId,
    string? VerifiedByUserName,
    int? ApprovedSpeciesId);
