using FaunaFinder.Client.Services.Api;
using Microsoft.JSInterop;

namespace FaunaFinder.Client.Services.Map;

public sealed class MapService : IMapService
{
    private readonly IJSRuntime _js;
    private readonly ApiConfiguration _apiConfig;

    public MapService(IJSRuntime js, ApiConfiguration apiConfig)
    {
        _js = js;
        _apiConfig = apiConfig;
    }

    public async Task InitMapAsync<T>(DotNetObjectReference<T> callbackHandler)
        where T : class
    {
        await _js.InvokeVoidAsync(
            "leafletInterop.initMap",
            callbackHandler,
            _apiConfig.BaseAddress
        );
    }

    public async Task SetDarkModeAsync(bool isDarkMode)
    {
        await _js.InvokeVoidAsync("leafletInterop.setDarkMode", isDarkMode);
    }

    public async Task ShowSearchRadiusAsync(double radiusMeters)
    {
        await _js.InvokeVoidAsync("leafletInterop.showSearchRadius", radiusMeters);
    }

    public async Task ClearSearchRadiusAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.clearSearchRadius");
    }

    public async Task ShowSpeciesLocationsAsync(
        string commonName,
        IEnumerable<SpeciesLocationData> locations
    )
    {
        var jsLocations = locations
            .Select(l => new JsSpeciesLocation(
                l.Latitude,
                l.Longitude,
                l.RadiusMeters,
                l.Description
            ))
            .ToArray();

        await _js.InvokeVoidAsync(
            "leafletInterop.showSpeciesLocations",
            commonName,
            (object)jsLocations
        );
    }

    public async Task ClearSpeciesLocationsAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.clearSpeciesLocations");
    }

    public async Task FocusOnLocationAsync(int index)
    {
        await _js.InvokeVoidAsync("leafletInterop.focusOnLocation", index);
    }

    public async Task FocusAllLocationsAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.focusAllLocations");
    }

    public async Task ShowNearbySpeciesAsync(IEnumerable<NearbySpeciesData> species)
    {
        var jsSpecies = species
            .Select(s => new JsNearbySpecies(
                s.Id,
                s.CommonName,
                s.ScientificName,
                s.DistanceMeters,
                s.Latitude,
                s.Longitude,
                s.RadiusMeters,
                s.LocationDescription
            ))
            .ToArray();

        await _js.InvokeVoidAsync("leafletInterop.showNearbySpecies", (object)jsSpecies);
    }

    public async Task FocusOnNearbySpeciesAsync(int index)
    {
        await _js.InvokeVoidAsync("leafletInterop.focusOnNearbySpecies", index);
    }

    public async Task ShowSpeciesLocationCirclesAsync(IEnumerable<NearbySpeciesData> species)
    {
        var jsSpecies = species
            .Select(s => new JsNearbySpecies(
                s.Id,
                s.CommonName,
                s.ScientificName,
                s.DistanceMeters,
                s.Latitude,
                s.Longitude,
                s.RadiusMeters,
                s.LocationDescription
            ))
            .ToArray();

        await _js.InvokeVoidAsync("leafletInterop.showSpeciesLocationCircles", (object)jsSpecies);
    }

    public async Task ClearSpeciesLocationCirclesAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.clearSpeciesLocationCircles");
    }

    public async Task ResetSpeciesColorsAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.resetSpeciesColors");
    }

    public async Task<List<SpeciesColorEntry>> GetSpeciesColorsAsync()
    {
        return await _js.InvokeAsync<List<SpeciesColorEntry>>("leafletInterop.getSpeciesColors");
    }

    public async Task DownloadFileAsync(byte[] fileBytes, string fileName, string contentType)
    {
        var base64 = Convert.ToBase64String(fileBytes);
        await _js.InvokeVoidAsync("downloadFile", base64, fileName, contentType);
    }

    public async Task SetSessionStorageAsync(string key, string value)
    {
        await _js.InvokeVoidAsync("sessionStorage.setItem", key, value);
    }

    public async Task<string?> GetSessionStorageAsync(string key)
    {
        return await _js.InvokeAsync<string?>("sessionStorage.getItem", key);
    }

    public async Task RemoveSessionStorageAsync(string key)
    {
        await _js.InvokeVoidAsync("sessionStorage.removeItem", key);
    }

    public async Task EnableDrawModeAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.enableDrawMode");
    }

    public async Task DisableDrawModeAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.disableDrawMode");
    }

    public async Task ClearDrawnCircleAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.clearDrawnCircle");
    }

    public async Task EnablePolygonDrawModeAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.enablePolygonDrawMode");
    }

    public async Task DisablePolygonDrawModeAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.disablePolygonDrawMode");
    }

    public async Task ClearDrawnPolygonAsync()
    {
        await _js.InvokeVoidAsync("leafletInterop.clearDrawnPolygon");
    }

    public async Task HighlightMunicipalityAsync(string countyCode)
    {
        await _js.InvokeVoidAsync("leafletInterop.highlightMunicipality", countyCode);
    }

    public async Task FocusOnMunicipalityAsync(string countyCode)
    {
        await _js.InvokeVoidAsync("leafletInterop.focusOnMunicipality", countyCode);
    }
}
