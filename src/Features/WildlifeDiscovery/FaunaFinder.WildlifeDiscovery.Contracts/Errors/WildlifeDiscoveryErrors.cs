namespace FaunaFinder.WildlifeDiscovery.Contracts.Errors;

public abstract record WildlifeDiscoveryError(string Message);

public sealed record ValidationError(string Message, IDictionary<string, string[]> Errors)
    : WildlifeDiscoveryError(Message);

public sealed record UnauthorizedError()
    : WildlifeDiscoveryError("You must be logged in to perform this action");

public sealed record ForbiddenError()
    : WildlifeDiscoveryError("You do not have permission to perform this action");

public sealed record NotFoundError(string ResourceType, int Id)
    : WildlifeDiscoveryError($"{ResourceType} with ID {Id} was not found");

public sealed record SightingNotFoundError(int Id)
    : WildlifeDiscoveryError($"Sighting with ID {Id} was not found");

public sealed record SpeciesNotFoundError(int Id)
    : WildlifeDiscoveryError($"Species with ID {Id} was not found");

public sealed record UserSpeciesNotFoundError(int Id)
    : WildlifeDiscoveryError($"User species with ID {Id} was not found");

public sealed record InvalidOperationError(string Details)
    : WildlifeDiscoveryError(Details);

public sealed record UnexpectedError(string Details)
    : WildlifeDiscoveryError("An unexpected error occurred");
