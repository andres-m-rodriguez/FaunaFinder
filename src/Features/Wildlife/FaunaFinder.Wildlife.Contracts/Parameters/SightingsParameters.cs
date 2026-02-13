namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record SightingsParameters(
    int PageSize = 20,
    int Page = 1,
    string? Status = null
);
