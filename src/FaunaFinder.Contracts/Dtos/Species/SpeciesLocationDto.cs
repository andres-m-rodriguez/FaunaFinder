namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesLocationDto(
    int Id,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string? Description
);
