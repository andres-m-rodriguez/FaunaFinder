namespace FaunaFinder.Identity.Contracts.Responses;

public sealed record UserInfo(
    int Id,
    string Email,
    string DisplayName,
    string Status,
    string Role);
