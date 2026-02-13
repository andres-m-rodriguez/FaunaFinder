namespace FaunaFinder.Pagination.Contracts;

/// <summary>
/// Base record for keyset (cursor-based) pagination with a strongly-typed cursor.
/// </summary>
/// <typeparam name="TKey">The type of the cursor key (typically int for entity IDs)</typeparam>
public record KeysetPagination<TKey>(
    int PageSize = 20,
    TKey? Cursor = default
) where TKey : struct;
