using FluentValidation;

namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed class VerifyUserSpeciesRequestValidator : AbstractValidator<VerifyUserSpeciesRequest>
{
    public VerifyUserSpeciesRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .MaximumLength(2000).WithMessage("Rejection reason cannot exceed 2000 characters");

        When(x => !x.Approve, () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required when rejecting");
        });
    }
}
