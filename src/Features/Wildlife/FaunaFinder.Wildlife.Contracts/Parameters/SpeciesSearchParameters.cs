namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record SpeciesSearchParameters(
    string? Query = null,
    int Limit = 10
);
