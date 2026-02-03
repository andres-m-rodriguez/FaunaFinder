using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Responses;
using OneOf;

namespace FaunaFinder.Identity.Contracts.Results;

[GenerateOneOf]
public partial class LoginResult : OneOfBase<UserInfo, InvalidCredentialsError, AccountLockedError, AccountNotApprovedError, ValidationError, UnexpectedError>;

[GenerateOneOf]
public partial class RegisterResult : OneOfBase<UserInfo, EmailAlreadyExistsError, RegistrationFailedError, ValidationError, UnexpectedError>;

[GenerateOneOf]
public partial class GetCurrentUserResult : OneOfBase<UserInfo, UnauthorizedError, UnexpectedError>;

[GenerateOneOf]
public partial class GetUsersResult : OneOfBase<UserInfo[], ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class GetPendingUsersResult : OneOfBase<UserInfo[], ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class UpdateUserStatusResult : OneOfBase<UserInfo, UserNotFoundError, ForbiddenError, ValidationError, UnexpectedError>;
