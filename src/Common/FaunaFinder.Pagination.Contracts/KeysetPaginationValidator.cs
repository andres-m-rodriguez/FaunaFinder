using FluentValidation;

namespace FaunaFinder.Pagination.Contracts;

/// <summary>
/// Reusable validator for keyset pagination parameters.
/// Ensures PageSize is within acceptable bounds.
/// </summary>
public class KeysetPaginationValidator<TKey> : AbstractValidator<KeysetPagination<TKey>>
    where TKey : struct
{
    public KeysetPaginationValidator(int maxPageSize = 100)
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, maxPageSize)
            .WithMessage($"PageSize must be between 1 and {maxPageSize}");
    }
}
