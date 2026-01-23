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
 * Represents a nearby species location with distance info for "Species Near Me" feature
 */
interface NearbySpeciesLocation {
    id: number;
    commonName: string;
    scientificName: string;
    distanceMeters: number;
    latitude: number;
    longitude: number;
    radiusMeters: number;
    locationDescription?: string;
}

/**
 * User location coordinates
 */
interface UserLocation {
    latitude: number;
    longitude: number;
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
    STATE: string;
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
    userLocation: UserLocation | null;
    searchRadiusCircle: L.Circle | null;
    nearbySpeciesMarkers: L.Circle[];
    speciesLocationCircles: L.Circle[];
    speciesColorMap: Map<number, string>;
    speciesColorPalette: string[];
    geojsonLoadedPromise: Promise<void> | null;
    geojsonLoadedResolve: (() => void) | null;
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
    selectMunicipality(county: string): Promise<void>;
    showSpeciesLocations(speciesName: string, locations: SpeciesLocation[]): void;
    clearSpeciesLocations(): void;
    focusOnLocation(index: number): void;
    focusAllLocations(): void;
    createLocateControl(): void;
    locateUser(): void;
    setLocateControlState(state: string): void;
    showLocationError(errorType: string): void;
    showSearchRadius(radiusMeters: number): void;
    clearSearchRadius(): void;
    showNearbySpecies(species: NearbySpeciesLocation[]): void;
    clearNearbySpeciesMarkers(): void;
    getUserLocation(): UserLocation | null;
    focusOnNearbySpecies(index: number): void;
    getSpeciesColor(speciesId: number, index: number): string;
    showSpeciesLocationCircles(species: NearbySpeciesLocation[]): void;
    clearSpeciesLocationCircles(): void;
    getSpeciesColors(): { id: number; color: string }[];
    resetSpeciesColors(): void;
}

interface Window {
    leafletInterop: LeafletInteropObject;
    downloadFile: (base64: string, fileName: string, contentType: string) => void;
}
