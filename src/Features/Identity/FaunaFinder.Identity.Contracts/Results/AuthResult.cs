using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Pagination.Contracts;
using OneOf;

namespace FaunaFinder.Identity.Contracts.Results;

public sealed record RegisterSuccess(string Message);

[GenerateOneOf]
public partial class LoginResult
    : OneOfBase<
        UserInfo,
        InvalidCredentialsError,
        AccountLockedError,
        AccountNotApprovedError,
        ValidationError,
        TooManyRequestsError,
        UnexpectedError
    >;

[GenerateOneOf]
public partial class RegisterResult
    : OneOfBase<
        RegisterSuccess,
        EmailAlreadyExistsError,
        RegistrationFailedError,
        ValidationError,
        TooManyRequestsError,
        UnexpectedError
    >;

[GenerateOneOf]
public partial class GetCurrentUserResult : OneOfBase<UserInfo, UnauthorizedError, UnexpectedError>;

[GenerateOneOf]
public partial class GetUsersResult : OneOfBase<UserInfo[], ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class GetAccessRequestsResult
    : OneOfBase<AccessRequestInfo[], ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class UpdateAccessRequestStatusResult
    : OneOfBase<
        AccessRequestInfo,
        AccessRequestNotFoundError,
        ForbiddenError,
        ValidationError,
        UnexpectedError
    >;

[GenerateOneOf]
public partial class GetUsersCursorPageResult
    : OneOfBase<CursorPage<UserInfo>, ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class GetAccessRequestByIdResult
    : OneOfBase<AccessRequestInfo, AccessRequestNotFoundError, ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class GetAccessRequestsCursorPageResult
    : OneOfBase<CursorPage<AccessRequestInfo>, ForbiddenError, UnexpectedError>;
