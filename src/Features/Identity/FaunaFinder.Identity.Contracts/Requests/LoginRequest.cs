namespace FaunaFinder.Identity.Contracts.Requests;

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false);
