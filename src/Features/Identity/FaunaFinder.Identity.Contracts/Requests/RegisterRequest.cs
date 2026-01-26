namespace FaunaFinder.Identity.Contracts.Requests;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName);
