namespace FaunaFinder.Contracts.Pagination;

public record KeysetPaginationParameters(int? Limit = 50, int? FromCursor = null);
