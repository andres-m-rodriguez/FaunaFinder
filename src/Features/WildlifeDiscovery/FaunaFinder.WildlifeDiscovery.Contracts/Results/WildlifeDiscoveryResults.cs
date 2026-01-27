using FaunaFinder.WildlifeDiscovery.Contracts.Errors;
using FaunaFinder.WildlifeDiscovery.Contracts.Responses;
using OneOf;

namespace FaunaFinder.WildlifeDiscovery.Contracts.Results;

[GenerateOneOf]
public partial class CreateSightingResult : OneOfBase<SightingDto, ValidationError, UnauthorizedError, SpeciesNotFoundError, UserSpeciesNotFoundError, UnexpectedError>;

[GenerateOneOf]
public partial class GetSightingResult : OneOfBase<SightingDto, NotFoundError, UnauthorizedError, UnexpectedError>;

[GenerateOneOf]
public partial class GetSightingsResult : OneOfBase<SightingListResult, UnauthorizedError, UnexpectedError>;

[GenerateOneOf]
public partial class ReviewSightingResult : OneOfBase<SightingDto, ValidationError, NotFoundError, UnauthorizedError, ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class CreateUserSpeciesResult : OneOfBase<UserSpeciesDto, ValidationError, UnauthorizedError, UnexpectedError>;

[GenerateOneOf]
public partial class GetUserSpeciesResult : OneOfBase<UserSpeciesDto, NotFoundError, UnauthorizedError, UnexpectedError>;

[GenerateOneOf]
public partial class VerifyUserSpeciesResult : OneOfBase<UserSpeciesDto, ValidationError, NotFoundError, UnauthorizedError, ForbiddenError, InvalidOperationError, UnexpectedError>;

[GenerateOneOf]
public partial class GetReviewQueueResult : OneOfBase<ReviewQueueDto, UnauthorizedError, ForbiddenError, UnexpectedError>;

[GenerateOneOf]
public partial class SearchSpeciesResult : OneOfBase<SpeciesSearchListResult, UnexpectedError>;

[GenerateOneOf]
public partial class GetSightingMediaResult : OneOfBase<MediaResult, NotFoundError, UnauthorizedError, UnexpectedError>;

// Wrapper types to avoid interface issues with OneOf
public sealed record SightingListResult(List<SightingListDto> Items);
public sealed record SpeciesSearchListResult(List<SpeciesSearchDto> Items);
public sealed record MediaResult(byte[] Data, string ContentType);
