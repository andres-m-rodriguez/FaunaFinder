using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record SpeciesParameters(
    int PageSize = 20,
    int? Cursor = null,
    string? Search = null,
    int? MunicipalityId = null,
    string? CategoryIds = null
) : KeysetPagination<int>(PageSize, Cursor);
