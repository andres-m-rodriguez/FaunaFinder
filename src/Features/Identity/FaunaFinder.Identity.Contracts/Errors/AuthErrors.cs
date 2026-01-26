namespace FaunaFinder.Identity.Contracts.Errors;

public abstract record AuthError(string Message);

public sealed record ValidationError(string Message, IDictionary<string, string[]> Errors)
    : AuthError(Message);

public sealed record InvalidCredentialsError()
    : AuthError("Invalid email or password");

public sealed record EmailAlreadyExistsError(string Email)
    : AuthError($"An account with email '{Email}' already exists");

public sealed record AccountLockedError(DateTimeOffset? LockoutEnd)
    : AuthError("Account is locked. Please try again later");

public sealed record AccountNotApprovedError()
    : AuthError("Account is pending approval");

public sealed record RegistrationFailedError(IEnumerable<string> Errors)
    : AuthError("Registration failed: " + string.Join(", ", Errors));

public sealed record UnauthorizedError()
    : AuthError("You must be logged in to perform this action");

public sealed record UnexpectedError(string Details)
    : AuthError("An unexpected error occurred");
