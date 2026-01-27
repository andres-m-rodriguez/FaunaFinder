namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed record VerifyUserSpeciesRequest(
    bool Approve,
    bool IsEndangered,
    int? ExistingSpeciesId,
    string? RejectionReason);
