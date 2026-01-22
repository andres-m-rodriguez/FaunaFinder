/// <reference types="leaflet" />

/**
 * Represents a species location with coordinates and radius
 */
interface SpeciesLocation {
    latitude: number;
    longitude: number;
    radiusMeters: number;
    description?: string;
}

/**
 * Theme colors for map styling
 */
interface ThemeColors {
    fillColor: string;
    borderColor: string;
    highlightFill: string;
    highlightBorder: string;
}

/**
 * Blazor .NET interop object reference
 */
interface DotNetObjectReference {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

/**
 * GeoJSON feature properties for Puerto Rico municipalities
 */
interface MunicipalityProperties {
    NAME: string;
    COUNTY: string;
}

/**
 * Leaflet interop object for Blazor integration
 */
interface LeafletInteropObject {
    map: L.Map | null;
    geojsonLayer: L.GeoJSON | null;
    dotNetHelper: DotNetObjectReference | null;
    isMobile: boolean;
    locationCircles: L.Circle[];
    tileLayer: L.TileLayer | null;
    isDarkMode: boolean;
    userLocationMarker: L.Marker | null;
    locateControl: HTMLElement | null;
    isLocating: boolean;
    lightTileUrl: string;
    darkTileUrl: string;
    lightTheme: ThemeColors;
    darkTheme: ThemeColors;
    initMap(dotNetHelper: DotNetObjectReference): void;
    setDarkMode(isDark: boolean): void;
    loadGeoJson(): void;
    getDefaultStyle(): L.PathOptions;
    defaultStyle(): L.PathOptions;
    highlightFeature(e: L.LeafletMouseEvent): void;
    resetHighlight(e: L.LeafletMouseEvent): void;
    highlightMunicipality(county: string): void;
    showSpeciesLocations(speciesName: string, locations: SpeciesLocation[]): void;
    clearSpeciesLocations(): void;
    focusOnLocation(index: number): void;
    focusAllLocations(): void;
    createLocateControl(): void;
    locateUser(): void;
    setLocateControlState(state: string): void;
    showLocationError(errorType: string): void;
}

interface Window {
    leafletInterop: LeafletInteropObject;
}
