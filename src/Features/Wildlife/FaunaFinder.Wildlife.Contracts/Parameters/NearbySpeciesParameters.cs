namespace FaunaFinder.Wildlife.Contracts.Parameters;

public record NearbySpeciesParameters(double Latitude, double Longitude, double RadiusMeters = 5000);
