namespace FaunaFinder.Contracts.Parameters;

public sealed record MunicipalityParameters(
    int PageSize = 12,
    int Page = 0,
    string? Search = null
);
