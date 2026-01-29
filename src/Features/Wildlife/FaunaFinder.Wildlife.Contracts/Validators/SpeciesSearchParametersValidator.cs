using FluentValidation;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.Contracts.Validators;

public sealed class SpeciesSearchParametersValidator : AbstractValidator<SpeciesSearchParameters>
{
    public SpeciesSearchParametersValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 50)
            .WithMessage("Limit must be between 1 and 50");
    }
}
