using FaunaFinder.i18n.Contracts;
using Microsoft.JSInterop;

namespace FaunaFinder.Client.Services.Map;

public interface IMapService
{
    Task InitMapAsync<T>(DotNetObjectReference<T> callbackHandler) where T : class;
    Task SetDarkModeAsync(bool isDarkMode);
    Task ShowSearchRadiusAsync(double radiusMeters);
    Task ClearSearchRadiusAsync();
    Task ShowSpeciesLocationsAsync(string commonName, IEnumerable<SpeciesLocationData> locations);
    Task ClearSpeciesLocationsAsync();
    Task FocusOnLocationAsync(int index);
    Task FocusAllLocationsAsync();
    Task ShowNearbySpeciesAsync(IEnumerable<NearbySpeciesData> species);
    Task FocusOnNearbySpeciesAsync(int index);
    Task ShowSpeciesLocationCirclesAsync(IEnumerable<NearbySpeciesData> species);
    Task ClearSpeciesLocationCirclesAsync();
    Task ResetSpeciesColorsAsync();
    Task<List<SpeciesColorEntry>> GetSpeciesColorsAsync();
    Task DownloadFileAsync(byte[] fileBytes, string fileName, string contentType);

    // Session storage methods for state preservation
    Task SetSessionStorageAsync(string key, string value);
    Task<string?> GetSessionStorageAsync(string key);
    Task RemoveSessionStorageAsync(string key);
}

public record SpeciesLocationData(
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string? Description);

public record NearbySpeciesData(
    int Id,
    IEnumerable<LocaleValue> CommonName,
    string ScientificName,
    double DistanceMeters,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string? LocationDescription);

public record SpeciesColorEntry(
    [property: System.Text.Json.Serialization.JsonPropertyName("id")] int Id,
    [property: System.Text.Json.Serialization.JsonPropertyName("color")] string Color);
