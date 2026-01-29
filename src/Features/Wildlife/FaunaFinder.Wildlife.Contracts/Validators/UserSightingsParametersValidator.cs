using FluentValidation;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.Contracts.Validators;

public sealed class UserSightingsParametersValidator : AbstractValidator<UserSightingsParameters>
{
    public UserSightingsParametersValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
