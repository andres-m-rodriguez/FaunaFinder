using FluentValidation;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.Contracts.Validators;

public sealed class SightingsParametersValidator : AbstractValidator<SightingsParameters>
{
    private static readonly string[] ValidStatuses = ["Pending", "Approved", "Rejected"];

    public SightingsParametersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Status)
            .Must(status => status is null || ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be one of: Pending, Approved, Rejected");
    }
}
