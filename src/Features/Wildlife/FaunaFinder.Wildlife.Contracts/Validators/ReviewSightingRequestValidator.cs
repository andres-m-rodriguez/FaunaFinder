using FluentValidation;
using FaunaFinder.Wildlife.Contracts.Requests;

namespace FaunaFinder.Wildlife.Contracts.Validators;

public sealed class ReviewSightingRequestValidator : AbstractValidator<ReviewSightingRequest>
{
    private static readonly string[] ValidReviewStatuses = ["Approved", "Rejected"];

    public ReviewSightingRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(status => ValidReviewStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be one of: Approved, Rejected");

        RuleFor(x => x.ReviewNotes)
            .MaximumLength(2000)
            .WithMessage("ReviewNotes cannot exceed 2000 characters");
    }
}
