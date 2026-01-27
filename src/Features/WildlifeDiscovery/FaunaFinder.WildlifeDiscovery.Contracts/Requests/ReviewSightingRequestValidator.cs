using FluentValidation;

namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed class ReviewSightingRequestValidator : AbstractValidator<ReviewSightingRequest>
{
    private static readonly string[] ValidStatuses = ["Approved", "Rejected"];

    public ReviewSightingRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => ValidStatuses.Contains(s)).WithMessage("Status must be 'Approved' or 'Rejected'");

        RuleFor(x => x.ReviewNotes)
            .MaximumLength(2000).WithMessage("Review notes cannot exceed 2000 characters");
    }
}
