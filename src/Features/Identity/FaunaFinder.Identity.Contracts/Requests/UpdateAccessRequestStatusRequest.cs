using FluentValidation;

namespace FaunaFinder.Identity.Contracts.Requests;

public sealed record UpdateAccessRequestStatusRequest(string Status);

public sealed class UpdateAccessRequestStatusRequestValidator
    : AbstractValidator<UpdateAccessRequestStatusRequest>
{
    private static readonly string[] ValidStatuses = ["Approved", "Rejected"];

    public UpdateAccessRequestStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be 'Approved' or 'Rejected'");
    }
}
