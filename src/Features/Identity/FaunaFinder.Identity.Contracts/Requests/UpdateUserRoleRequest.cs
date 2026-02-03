using FluentValidation;

namespace FaunaFinder.Identity.Contracts.Requests;

public sealed record UpdateUserRoleRequest(string Role);

public sealed class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    private static readonly string[] ValidRoles = ["Viewer", "Contributor", "Admin", "Student", "Teacher"];

    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => ValidRoles.Contains(r))
            .WithMessage("Role must be one of: Viewer, Contributor, Admin, Student, Teacher");
    }
}
