namespace FaunaFinder.Wildlife.Contracts.Requests;

public sealed record ReviewSightingRequest(
    string Status,
    string? ReviewNotes = null
);
