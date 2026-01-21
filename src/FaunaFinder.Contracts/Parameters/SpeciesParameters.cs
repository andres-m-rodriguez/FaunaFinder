using FaunaFinder.Contracts.Pagination;

namespace FaunaFinder.Contracts.Parameters;

public sealed record SpeciesParameters(
    int? Limit = 50,
    int? FromCursor = null,
    string? Search = null,
    int? MunicipalityId = null
) : KeysetPaginationParameters(Limit, FromCursor);
