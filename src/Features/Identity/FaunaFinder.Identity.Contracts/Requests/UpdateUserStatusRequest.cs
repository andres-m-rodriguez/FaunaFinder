using FluentValidation;

namespace FaunaFinder.Identity.Contracts.Requests;

public sealed record UpdateUserStatusRequest(string Status);

public sealed class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
    private static readonly string[] ValidStatuses = ["Approved", "Rejected"];

    public UpdateUserStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be 'Approved' or 'Rejected'");
    }
}
