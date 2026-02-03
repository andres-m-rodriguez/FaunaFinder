namespace FaunaFinder.Identity.Contracts.Responses;

public sealed record AccessRequestInfo(
    int Id,
    string Email,
    string DisplayName,
    string? Message,
    string Status,
    DateTime CreatedAt);
