namespace FaunaFinder.Contracts.Parameters;

public sealed record SpeciesParameters(
    int PageSize = 12,
    int Page = 0,
    string? Search = null,
    int? MunicipalityId = null
);
