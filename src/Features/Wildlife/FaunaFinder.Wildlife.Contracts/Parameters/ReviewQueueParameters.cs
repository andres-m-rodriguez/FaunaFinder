namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record ReviewQueueParameters(
    int Page = 1,
    int PageSize = 20
);
