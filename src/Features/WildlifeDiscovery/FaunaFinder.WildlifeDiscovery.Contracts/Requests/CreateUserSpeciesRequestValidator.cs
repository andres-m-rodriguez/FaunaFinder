using FluentValidation;

namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed class CreateUserSpeciesRequestValidator : AbstractValidator<CreateUserSpeciesRequest>
{
    public CreateUserSpeciesRequestValidator()
    {
        RuleFor(x => x.CommonName)
            .NotEmpty().WithMessage("Common name is required")
            .MaximumLength(200).WithMessage("Common name cannot exceed 200 characters");

        RuleFor(x => x.ScientificName)
            .MaximumLength(200).WithMessage("Scientific name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }
}
