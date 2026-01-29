namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record SightingsParameters(
    int Page = 1,
    int PageSize = 20,
    string? Status = null
);
