namespace FaunaFinder.WildlifeDiscovery.Contracts.Responses;

public sealed record ReviewQueueDto(
    IReadOnlyList<SightingListDto> Sightings,
    IReadOnlyList<UserSpeciesDto> PendingSpecies,
    int TotalSightings,
    int TotalPendingSpecies);
