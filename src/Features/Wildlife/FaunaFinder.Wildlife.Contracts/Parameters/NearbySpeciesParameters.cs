namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record NearbySpeciesParameters(
    double Latitude,
    double Longitude,
    double RadiusMeters = 5000
);
