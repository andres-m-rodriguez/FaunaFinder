using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record MunicipalityParameters(
    int PageSize = 20,
    int? Cursor = null,
    string? Search = null
) : KeysetPagination<int>(PageSize, Cursor);
