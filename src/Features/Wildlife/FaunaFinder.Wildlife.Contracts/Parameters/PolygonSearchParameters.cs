using System.Text.Json.Serialization;

namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record PolygonCoordinate(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude
);

public sealed record PolygonSearchParameters(
    [property: JsonPropertyName("coordinates")] IReadOnlyList<PolygonCoordinate> Coordinates
);
