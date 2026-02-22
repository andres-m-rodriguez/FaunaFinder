/// <reference types="leaflet" />

// ============================================================================
// Type Definitions
// ============================================================================

/**
 * Properties of a municipality from GeoJSON features
 */
interface MunicipalityProperties {
    STATE: string;
    COUNTY: string;
    NAME: string;
}

/**
 * A location where a species has been observed
 */
interface SpeciesLocation {
    latitude: number;
    longitude: number;
    radiusMeters: number;
    description?: string;
}

/**
 * A species found near the user's location
 */
interface NearbySpecies {
    id: number;
    commonName: string;
    scientificName: string;
    latitude: number;
    longitude: number;
    radiusMeters: number;
    distanceMeters: number;
    locationDescription?: string;
}

/**
 * User's geolocation coordinates
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
 * Species color mapping entry
 */
interface SpeciesColorEntry {
    id: number;
    color: string;
}

/**
 * Blazor .NET interop helper interface
 */
interface DotNetHelper {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

/**
 * GeoJSON Feature with municipality properties
 */
interface MunicipalityFeature extends GeoJSON.Feature<GeoJSON.Geometry, MunicipalityProperties> {}

/**
 * GeoJSON FeatureCollection with municipality features
 */
interface MunicipalityGeoJSON extends GeoJSON.FeatureCollection<GeoJSON.Geometry, MunicipalityProperties> {}

/**
 * Leaflet layer style options
 */
interface LayerStyle {
    fillColor: string;
    weight: number;
    color: string;
    fillOpacity: number;
    dashArray?: string;
}

/**
 * Location error message types
 */
type LocationErrorType =
    | 'geolocation_unsupported'
    | 'permission_denied'
    | 'position_unavailable'
    | 'timeout'
    | 'unknown';

/**
 * Locate control state
 */
type LocateControlState = 'loading' | 'default';

// Extend Window interface for global functions
interface Window {
    leafletInterop: LeafletInterop;
    downloadFile: (base64: string, fileName: string, contentType: string) => void;
    initSightingMap: (containerId: string, latitude: number, longitude: number) => void;
}

// Extend Leaflet types for GeoJSON layer with feature property
interface GeoJSONLayerWithFeature extends L.Layer {
    feature?: MunicipalityFeature;
    setStyle(style: L.PathOptions): this;
    getBounds(): L.LatLngBounds;
}

// ============================================================================
// Leaflet Interop Interface
// ============================================================================

/**
 * Leaflet Interop class for Blazor integration
 */
interface LeafletInterop {
    map: L.Map | null;
    geojsonLayer: L.GeoJSON | null;
    dotNetHelper: DotNetHelper | null;
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
    apiBaseUrl: string;
    lightTileUrl: string;
    darkTileUrl: string;
    lightTheme: ThemeColors;
    darkTheme: ThemeColors;
    speciesColorPalette: string[];
    loadingOverlay: HTMLElement | null;
    tilesLoading: number;
    ariaLiveRegion: HTMLElement | null;
    isDrawMode: boolean;
    drawCircle: L.Circle | null;
    drawStartPoint: L.LatLng | null;
    isPolygonDrawMode: boolean;
    drawPolygon: L.Polygon | null;
    drawPolygonPoints: L.LatLng[];
    drawPolygonMarkers: L.CircleMarker[];
    drawPolygonPreviewLine: L.Polyline | null;

    setApiBaseUrl(url: string): void;
    initMap(dotNetHelper: DotNetHelper, apiBaseUrl?: string): void;
    createLocateControl(): void;
    createLoadingOverlay(): void;
    showLoadingOverlay(): void;
    hideLoadingOverlay(): void;
    createAriaLiveRegion(): void;
    announceToScreenReader(message: string): void;
    locateUser(): void;
    setLocateControlState(state: LocateControlState): void;
    showLocationError(errorType: LocationErrorType): void;
    setDarkMode(isDark: boolean): void;
    loadGeoJson(): void;
    getDefaultStyle(): LayerStyle;
    defaultStyle(): LayerStyle;
    highlightFeature(e: L.LeafletMouseEvent): void;
    resetHighlight(e: L.LeafletMouseEvent): void;
    highlightMunicipality(county: string): void;
    focusOnMunicipality(county: string): void;
    showSpeciesLocations(speciesName: string, locations: SpeciesLocation[]): void;
    clearSpeciesLocations(): void;
    focusOnLocation(index: number): void;
    focusAllLocations(): void;
    showSearchRadius(radiusMeters: number): void;
    clearSearchRadius(): void;
    showNearbySpecies(species: NearbySpecies[]): void;
    clearNearbySpeciesMarkers(): void;
    getUserLocation(): UserLocation | null;
    focusOnNearbySpecies(index: number): void;
    getSpeciesColor(speciesId: number, index: number): string;
    showSpeciesLocationCircles(species: NearbySpecies[]): void;
    clearSpeciesLocationCircles(): void;
    getSpeciesColors(): SpeciesColorEntry[];
    resetSpeciesColors(): void;
    initSightingMap(containerId: string, latitude: number, longitude: number): void;
    enableDrawMode(): void;
    disableDrawMode(): void;
    clearDrawnCircle(): void;
    enablePolygonDrawMode(): void;
    disablePolygonDrawMode(): void;
    clearDrawnPolygon(): void;
    updateDrawingPolygon(): void;
    finishPolygon(): void;
}

// ============================================================================
// Implementation
// ============================================================================

window.leafletInterop = {
    map: null,
    geojsonLayer: null,
    dotNetHelper: null,
    isMobile: false,
    locationCircles: [],
    tileLayer: null,
    isDarkMode: false,
    userLocationMarker: null,
    locateControl: null,
    isLocating: false,
    userLocation: null,
    searchRadiusCircle: null,
    nearbySpeciesMarkers: [],
    speciesLocationCircles: [],
    speciesColorMap: new Map<number, string>(),
    apiBaseUrl: '',
    loadingOverlay: null,
    tilesLoading: 0,
    ariaLiveRegion: null,
    isDrawMode: false,
    drawCircle: null,
    drawStartPoint: null,
    isPolygonDrawMode: false,
    drawPolygon: null,
    drawPolygonPoints: [],
    drawPolygonMarkers: [],
    drawPolygonPreviewLine: null,
    lightTileUrl: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
    darkTileUrl: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
    lightTheme: {
        fillColor: '#d8f3dc',
        borderColor: '#52b788',
        highlightFill: '#40916c',
        highlightBorder: '#1b4332'
    },
    darkTheme: {
        fillColor: '#52b788',
        borderColor: '#95d5b2',
        highlightFill: '#b7e4c7',
        highlightBorder: '#d8f3dc'
    },
    speciesColorPalette: [
        '#e63946',
        '#f4a261',
        '#2a9d8f',
        '#e9c46a',
        '#264653',
        '#9b5de5',
        '#00bbf9',
        '#f15bb5',
        '#00f5d4',
        '#fee440',
        '#8338ec',
        '#fb5607',
        '#3a86ff',
        '#ff006e',
        '#06d6a0',
    ],

    setApiBaseUrl(url: string): void {
        this.apiBaseUrl = url.replace(/\/$/, '');
    },

    initMap(dotNetHelper: DotNetHelper, apiBaseUrl?: string): void {
        // Clean up existing map if re-initializing (e.g., after language change)
        if (this.map) {
            this.map.remove();
            this.map = null;
            this.tileLayer = null;
            this.geojsonLayer = null;
            this.locateControl = null;
            this.userLocationMarker = null;
            this.searchRadiusCircle = null;
            this.locationCircles = [];
            this.nearbySpeciesMarkers = [];
            this.speciesLocationCircles = [];
        }

        this.dotNetHelper = dotNetHelper;
        if (apiBaseUrl) {
            this.apiBaseUrl = apiBaseUrl.replace(/\/$/, '');
        }
        this.isMobile = window.innerWidth < 640;

        const savedDarkMode = localStorage.getItem('faunafinder-darkmode');
        if (savedDarkMode !== null) {
            this.isDarkMode = savedDarkMode === 'true';
        } else {
            this.isDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
        }

        const PR_BOUNDS: L.LatLngBoundsExpression = [
            [17.1176, -67.9426],
            [19.42, -64.9007]
        ];

        const zoom = this.isMobile ? 8 : 9;
        this.map = L.map('map', {
            center: [18.15, -66.5],
            zoom: zoom,
            maxBounds: PR_BOUNDS,
            maxBoundsViscosity: 1.0,
            scrollWheelZoom: !this.isMobile
        });

        // Add ARIA attributes to map container for accessibility
        const mapContainer = this.map.getContainer();
        mapContainer.setAttribute('role', 'application');
        mapContainer.setAttribute('aria-label', 'Interactive map of Puerto Rico municipalities. Use mouse or touch to navigate.');
        mapContainer.setAttribute('tabindex', '0');

        // Create ARIA live region for announcements
        this.createAriaLiveRegion();

        // Add ARIA labels to zoom controls after a short delay to ensure they're rendered
        setTimeout(() => {
            const zoomIn = document.querySelector('.leaflet-control-zoom-in');
            const zoomOut = document.querySelector('.leaflet-control-zoom-out');
            if (zoomIn) {
                zoomIn.setAttribute('aria-label', 'Zoom in');
                zoomIn.setAttribute('role', 'button');
            }
            if (zoomOut) {
                zoomOut.setAttribute('aria-label', 'Zoom out');
                zoomOut.setAttribute('role', 'button');
            }
        }, 100);

        const tileUrl = this.isDarkMode ? this.darkTileUrl : this.lightTileUrl;
        const attribution = this.isDarkMode
            ? '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>'
            : '&copy; OpenStreetMap';

        this.tileLayer = L.tileLayer(tileUrl, {
            attribution: attribution,
            noWrap: true,
            minZoom: 7,
            maxZoom: 16
        }).addTo(this.map);

        // Set up tile loading events for loading overlay
        this.createLoadingOverlay();
        this.showLoadingOverlay();

        this.tileLayer.on('loading', () => {
            this.tilesLoading++;
            this.showLoadingOverlay();
        });

        this.tileLayer.on('load', () => {
            this.tilesLoading = Math.max(0, this.tilesLoading - 1);
            if (this.tilesLoading === 0) {
                this.hideLoadingOverlay();
            }
        });

        this.loadGeoJson();
        this.createLocateControl();

        window.addEventListener('resize', () => {
            this.isMobile = window.innerWidth < 640;
        });
    },

    createLocateControl(): void {
        const self = this;

        const LocateControl = L.Control.extend({
            options: {
                position: 'topleft' as L.ControlPosition
            },

            onAdd(): HTMLElement {
                const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-locate');
                const button = L.DomUtil.create('a', 'leaflet-control-locate-button', container) as HTMLAnchorElement;
                button.href = '#';
                button.title = 'Locate me';
                button.setAttribute('role', 'button');
                button.setAttribute('aria-label', 'Locate me');
                button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm8.94 3A8.994 8.994 0 0 0 13 3.06V1h-2v2.06A8.994 8.994 0 0 0 3.06 11H1v2h2.06A8.994 8.994 0 0 0 11 20.94V23h2v-2.06A8.994 8.994 0 0 0 20.94 13H23v-2h-2.06zM12 19c-3.87 0-7-3.13-7-7s3.13-7 7-7 7 3.13 7 7-3.13 7-7 7z"/></svg>';

                L.DomEvent.disableClickPropagation(container);
                L.DomEvent.on(button, 'click', function(e) {
                    L.DomEvent.preventDefault(e);
                    self.locateUser();
                });

                self.locateControl = container;
                return container;
            }
        });

        new LocateControl().addTo(this.map!);
    },

    createLoadingOverlay(): void {
        const mapContainer = document.getElementById('map');
        if (!mapContainer) return;

        // Remove existing overlay if present
        const existing = document.getElementById('map-loading-overlay');
        if (existing) {
            existing.remove();
        }

        const overlay = document.createElement('div');
        overlay.id = 'map-loading-overlay';
        overlay.className = 'map-loading-overlay';
        overlay.innerHTML = `
            <div class="map-loading-content">
                <div class="map-loading-spinner"></div>
                <span class="map-loading-text">Loading map...</span>
            </div>
        `;
        mapContainer.appendChild(overlay);
        this.loadingOverlay = overlay;
    },

    showLoadingOverlay(): void {
        if (this.loadingOverlay) {
            this.loadingOverlay.classList.add('visible');
        }
    },

    hideLoadingOverlay(): void {
        if (this.loadingOverlay) {
            this.loadingOverlay.classList.remove('visible');
            // Remove after fade animation
            setTimeout(() => {
                if (this.loadingOverlay && !this.loadingOverlay.classList.contains('visible')) {
                    this.loadingOverlay.style.display = 'none';
                }
            }, 300);
        }
    },

    createAriaLiveRegion(): void {
        // Remove existing live region if present
        const existing = document.getElementById('map-aria-live');
        if (existing) {
            existing.remove();
        }

        // Create ARIA live region for screen reader announcements
        const liveRegion = document.createElement('div');
        liveRegion.id = 'map-aria-live';
        liveRegion.setAttribute('role', 'status');
        liveRegion.setAttribute('aria-live', 'polite');
        liveRegion.setAttribute('aria-atomic', 'true');
        liveRegion.className = 'sr-only';
        liveRegion.style.cssText = 'position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;';
        document.body.appendChild(liveRegion);
        this.ariaLiveRegion = liveRegion;
    },

    announceToScreenReader(message: string): void {
        if (this.ariaLiveRegion) {
            // Clear and set new message to trigger announcement
            this.ariaLiveRegion.textContent = '';
            setTimeout(() => {
                if (this.ariaLiveRegion) {
                    this.ariaLiveRegion.textContent = message;
                }
            }, 100);
        }
    },

    locateUser(): void {
        if (this.isLocating) return;

        const self = this;

        if (!navigator.geolocation) {
            self.showLocationError('geolocation_unsupported');
            return;
        }

        this.isLocating = true;
        this.setLocateControlState('loading');

        navigator.geolocation.getCurrentPosition(
            function(position: GeolocationPosition) {
                self.isLocating = false;
                self.setLocateControlState('default');

                const lat = position.coords.latitude;
                const lng = position.coords.longitude;

                self.userLocation = { latitude: lat, longitude: lng };

                if (self.userLocationMarker) {
                    self.map!.removeLayer(self.userLocationMarker);
                }

                const userIcon = L.divIcon({
                    className: 'user-location-marker',
                    html: '<div class="user-location-pulse"></div><div class="user-location-dot"></div>',
                    iconSize: [24, 24],
                    iconAnchor: [12, 12]
                });

                self.userLocationMarker = L.marker([lat, lng], { icon: userIcon })
                    .addTo(self.map!)
                    .bindPopup('You are here');

                self.map!.flyTo([lat, lng], 14, {
                    duration: 1.5
                });

                // Announce location found to screen readers
                self.announceToScreenReader('Your location has been found. Map centered on your current position.');

                self.dotNetHelper?.invokeMethodAsync('OnUserLocationFound', lat, lng);
            },
            function(error: GeolocationPositionError) {
                self.isLocating = false;
                self.setLocateControlState('default');

                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        self.showLocationError('permission_denied');
                        break;
                    case error.POSITION_UNAVAILABLE:
                        self.showLocationError('position_unavailable');
                        break;
                    case error.TIMEOUT:
                        self.showLocationError('timeout');
                        break;
                    default:
                        self.showLocationError('unknown');
                        break;
                }
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 60000
            }
        );
    },

    setLocateControlState(state: LocateControlState): void {
        if (!this.locateControl) return;

        const button = this.locateControl.querySelector('.leaflet-control-locate-button') as HTMLElement | null;
        if (!button) return;

        if (state === 'loading') {
            button.classList.add('loading');
            button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor" class="spin"><path d="M12 4V2A10 10 0 0 0 2 12h2a8 8 0 0 1 8-8z"/></svg>';
        } else {
            button.classList.remove('loading');
            button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm8.94 3A8.994 8.994 0 0 0 13 3.06V1h-2v2.06A8.994 8.994 0 0 0 3.06 11H1v2h2.06A8.994 8.994 0 0 0 11 20.94V23h2v-2.06A8.994 8.994 0 0 0 20.94 13H23v-2h-2.06zM12 19c-3.87 0-7-3.13-7-7s3.13-7 7-7 7 3.13 7 7-3.13 7-7 7z"/></svg>';
        }
    },

    showLocationError(errorType: LocationErrorType): void {
        const messages: Record<LocationErrorType, string> = {
            'geolocation_unsupported': 'Geolocation is not supported by your browser.',
            'permission_denied': 'Location access was denied. Please enable location permissions.',
            'position_unavailable': 'Unable to determine your location.',
            'timeout': 'Location request timed out. Please try again.',
            'unknown': 'An unknown error occurred while getting your location.'
        };

        const message = messages[errorType] || messages['unknown'];

        if (this.map) {
            L.popup()
                .setLatLng(this.map.getCenter())
                .setContent('<div class="location-error-popup"><strong>Location Error</strong><br/>' + message + '</div>')
                .openOn(this.map);
        }
    },

    setDarkMode(isDark: boolean): void {
        this.isDarkMode = isDark;

        if (this.map) {
            if (this.tileLayer) {
                this.map.removeLayer(this.tileLayer);
            }

            // Show loading overlay while tiles load
            if (this.loadingOverlay) {
                this.loadingOverlay.style.display = '';
                this.showLoadingOverlay();
            }

            const tileUrl = isDark ? this.darkTileUrl : this.lightTileUrl;
            const attribution = isDark
                ? '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>'
                : '&copy; OpenStreetMap';

            this.tileLayer = L.tileLayer(tileUrl, {
                attribution: attribution,
                noWrap: true,
                minZoom: 7,
                maxZoom: 16
            }).addTo(this.map);

            // Set up tile loading events for the new layer
            this.tileLayer.on('loading', () => {
                this.tilesLoading++;
                this.showLoadingOverlay();
            });

            this.tileLayer.on('load', () => {
                this.tilesLoading = Math.max(0, this.tilesLoading - 1);
                if (this.tilesLoading === 0) {
                    this.hideLoadingOverlay();
                }
            });

            this.tileLayer.bringToBack();
        }

        if (this.geojsonLayer) {
            this.geojsonLayer.setStyle(this.getDefaultStyle());
        }
    },

    loadGeoJson(): void {
        const self = this;
        const CACHE_KEY = 'pr-municipios-geojson';
        const VERSION_KEY = 'pr-municipios-version';
        const CURRENT_VERSION = 'v1';

        const processGeoJson = (data: MunicipalityGeoJSON): void => {
            self.geojsonLayer = L.geoJSON(data, {
                style: () => self.getDefaultStyle(),
                onEachFeature: (feature: GeoJSON.Feature<GeoJSON.Geometry, MunicipalityProperties>, layer: L.Layer) => {
                    const props = feature.properties;
                    const name = props.NAME;
                    const state = props.STATE;
                    const county = props.COUNTY;
                    const geoJsonId = state + county;

                    (layer as L.GeoJSON).bindTooltip(name, {
                        direction: 'center',
                        className: 'municipality-tooltip'
                    });

                    // Add ARIA attributes to the layer element for accessibility
                    const layerElement = (layer as any).getElement?.();
                    if (layerElement) {
                        layerElement.setAttribute('role', 'button');
                        layerElement.setAttribute('aria-label', `Municipality: ${name}. Click to view species.`);
                        layerElement.setAttribute('tabindex', '0');
                    }

                    layer.on({
                        mouseover: (e: L.LeafletMouseEvent) => self.highlightFeature(e),
                        mouseout: (e: L.LeafletMouseEvent) => self.resetHighlight(e),
                        click: () => {
                            // Ignore clicks during draw mode
                            if (self.isDrawMode || self.isPolygonDrawMode) return;

                            // Announce municipality selection to screen readers
                            self.announceToScreenReader(`Selected ${name} municipality. Loading species information.`);
                            self.dotNetHelper?.invokeMethodAsync('OnMunicipalityClick', geoJsonId, name);
                        }
                    });
                }
            }).addTo(self.map!);

            console.log('GeoJSON loaded successfully with', data.features.length, 'features');

            // Notify Blazor that the map is fully loaded
            self.dotNetHelper?.invokeMethodAsync('OnMapReady');
        };

        // Check localStorage cache first
        const cached = localStorage.getItem(CACHE_KEY);
        const cacheVersion = localStorage.getItem(VERSION_KEY);

        if (cached && cacheVersion === CURRENT_VERSION) {
            try {
                const data = JSON.parse(cached) as MunicipalityGeoJSON;
                if (data.features && data.features.length > 0) {
                    console.log('GeoJSON loaded from localStorage cache');
                    processGeoJson(data);
                    return;
                }
            } catch (e) {
                console.warn('Failed to parse cached GeoJSON, fetching from API');
            }
        }

        // Load from API endpoint
        const apiUrl = this.apiBaseUrl ? this.apiBaseUrl + '/api/municipalities/geojson' : '/api/municipalities/geojson';
        fetch(apiUrl)
            .then((r: Response) => {
                if (!r.ok) throw new Error('API returned ' + r.status);
                return r.json();
            })
            .then((data: MunicipalityGeoJSON) => {
                if (data.features && data.features.length > 0) {
                    // Store in localStorage for future use
                    try {
                        localStorage.setItem(CACHE_KEY, JSON.stringify(data));
                        localStorage.setItem(VERSION_KEY, CURRENT_VERSION);
                        console.log('GeoJSON cached to localStorage');
                    } catch (e) {
                        console.warn('Failed to cache GeoJSON to localStorage:', (e as Error).message);
                    }
                    processGeoJson(data);
                } else {
                    throw new Error('No features in API response');
                }
            })
            .catch((err: Error) => {
                console.error('GeoJSON load error:', err);
                if (self.map) {
                    L.marker([18.15, -66.5]).addTo(self.map).bindPopup('GeoJSON error: ' + err.message).openPopup();
                }
            });
    },

    getDefaultStyle(): LayerStyle {
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        return {
            fillColor: theme.fillColor,
            weight: 1,
            color: theme.borderColor,
            fillOpacity: 0.3
        };
    },

    defaultStyle(): LayerStyle {
        return this.getDefaultStyle();
    },

    highlightFeature(e: L.LeafletMouseEvent): void {
        // Skip hover effects when in draw mode
        if (this.isDrawMode) {
            const layer = e.target as L.Path & { closeTooltip?: () => void };
            layer.closeTooltip?.();
            return;
        }

        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        const layer = e.target as L.Path;
        layer.setStyle({
            fillColor: theme.highlightFill,
            weight: 2,
            color: theme.highlightBorder,
            fillOpacity: 0.6
        });
    },

    resetHighlight(e: L.LeafletMouseEvent): void {
        // Skip hover effects when in draw mode
        if (this.isDrawMode) return;

        if (this.geojsonLayer) {
            this.geojsonLayer.resetStyle(e.target as L.Path);
        }
    },

    highlightMunicipality(county: string): void {
        if (!this.geojsonLayer) return;

        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        const self = this;

        this.geojsonLayer.eachLayer((layer: L.Layer) => {
            const geoLayer = layer as GeoJSONLayerWithFeature;
            const feature = geoLayer.feature;

            if (feature && feature.properties) {
                const props = feature.properties;

                if (props.COUNTY === county) {
                    geoLayer.setStyle({
                        fillColor: theme.highlightFill,
                        weight: 2,
                        color: theme.highlightBorder,
                        fillOpacity: 0.6
                    });
                } else {
                    self.geojsonLayer!.resetStyle(layer as L.Path);
                }
            }
        });
    },

    focusOnMunicipality(county: string): void {
        if (!this.geojsonLayer) return;

        this.geojsonLayer.eachLayer((layer: L.Layer) => {
            const geoLayer = layer as GeoJSONLayerWithFeature;
            const feature = geoLayer.feature;

            if (feature && feature.properties && feature.properties.COUNTY === county) {
                const bounds = geoLayer.getBounds();
                this.map!.flyToBounds(bounds, {
                    padding: [50, 50],
                    duration: 0.8
                });
            }
        });
    },

    showSpeciesLocations(speciesName: string, locations: SpeciesLocation[]): void {
        this.clearSpeciesLocations();

        if (!locations || locations.length === 0) return;

        const self = this;
        locations.forEach((loc: SpeciesLocation) => {
            const circle = L.circle([loc.latitude, loc.longitude], {
                radius: loc.radiusMeters,
                fillColor: '#ef4444',
                color: '#dc2626',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map!);

            const popupContent = loc.description
                ? `<strong>${speciesName}</strong><br/>${loc.description}`
                : `<strong>${speciesName}</strong>`;

            circle.bindPopup(popupContent);
            self.locationCircles.push(circle);
        });

        if (this.locationCircles.length > 0) {
            const group = L.featureGroup(this.locationCircles);
            this.map!.flyToBounds(group.getBounds(), {
                padding: [50, 50],
                duration: 0.8
            });
        }
    },

    clearSpeciesLocations(): void {
        const self = this;
        this.locationCircles.forEach((circle: L.Circle) => {
            self.map!.removeLayer(circle);
        });
        this.locationCircles = [];
    },

    focusOnLocation(index: number): void {
        if (index >= 0 && index < this.locationCircles.length) {
            const circle = this.locationCircles[index];
            this.map!.flyToBounds(circle.getBounds(), {
                padding: [50, 50],
                maxZoom: 14,
                duration: 0.8
            });
            // Open popup after animation completes
            setTimeout(() => circle.openPopup(), 800);
        }
    },

    focusAllLocations(): void {
        if (this.locationCircles.length > 0) {
            const group = L.featureGroup(this.locationCircles);
            this.map!.flyToBounds(group.getBounds(), {
                padding: [50, 50],
                duration: 0.8
            });
            this.locationCircles.forEach((circle: L.Circle) => circle.closePopup());
        }
    },

    showSearchRadius(radiusMeters: number): void {
        if (!this.userLocation || !this.map) return;

        if (this.searchRadiusCircle) {
            this.map.removeLayer(this.searchRadiusCircle);
        }

        this.searchRadiusCircle = L.circle([this.userLocation.latitude, this.userLocation.longitude], {
            radius: radiusMeters,
            fillColor: '#3b82f6',
            color: '#2563eb',
            weight: 2,
            fillOpacity: 0.1,
            dashArray: '5, 5'
        }).addTo(this.map);

        this.map.flyToBounds(this.searchRadiusCircle.getBounds(), {
            padding: [50, 50],
            duration: 0.8
        });
    },

    clearSearchRadius(): void {
        if (this.searchRadiusCircle && this.map) {
            this.map.removeLayer(this.searchRadiusCircle);
            this.searchRadiusCircle = null;
        }
        this.clearNearbySpeciesMarkers();
    },

    showNearbySpecies(species: NearbySpecies[]): void {
        this.clearNearbySpeciesMarkers();

        const speciesArray = Array.isArray(species) ? species : [];
        if (speciesArray.length === 0 || !this.map) return;

        const self = this;
        speciesArray.forEach((s: NearbySpecies) => {
            const circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: '#10b981',
                color: '#059669',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map!);

            const distanceText = s.distanceMeters < 1000
                ? `${Math.round(s.distanceMeters)}m away`
                : `${(s.distanceMeters / 1000).toFixed(1)}km away`;

            const popupContent = `
                <div class="nearby-species-popup">
                    <strong>${s.commonName}</strong><br/>
                    <em>${s.scientificName}</em><br/>
                    <span class="distance">${distanceText}</span>
                    ${s.locationDescription ? `<br/><small>${s.locationDescription}</small>` : ''}
                </div>
            `;

            circle.bindPopup(popupContent);
            self.nearbySpeciesMarkers.push(circle);
        });
    },

    clearNearbySpeciesMarkers(): void {
        const self = this;
        this.nearbySpeciesMarkers.forEach((marker: L.Circle) => {
            self.map!.removeLayer(marker);
        });
        this.nearbySpeciesMarkers = [];
    },

    getUserLocation(): UserLocation | null {
        return this.userLocation;
    },

    focusOnNearbySpecies(index: number): void {
        if (index >= 0 && index < this.nearbySpeciesMarkers.length) {
            const circle = this.nearbySpeciesMarkers[index];
            this.map!.flyToBounds(circle.getBounds(), {
                padding: [50, 50],
                maxZoom: 14,
                duration: 0.8
            });
            // Open popup after animation completes
            setTimeout(() => circle.openPopup(), 800);
        }
    },

    getSpeciesColor(speciesId: number, index: number): string {
        if (this.speciesColorMap.has(speciesId)) {
            return this.speciesColorMap.get(speciesId)!;
        }
        const color = this.speciesColorPalette[index % this.speciesColorPalette.length];
        this.speciesColorMap.set(speciesId, color);
        return color;
    },

    showSpeciesLocationCircles(species: NearbySpecies[]): void {
        this.clearSpeciesLocationCircles();

        const speciesArray = Array.isArray(species) ? species : [];
        if (speciesArray.length === 0 || !this.map) return;

        const self = this;
        const uniqueSpecies = new Map<number, number>();

        speciesArray.forEach((s: NearbySpecies) => {
            if (!uniqueSpecies.has(s.id)) {
                uniqueSpecies.set(s.id, uniqueSpecies.size);
            }
        });

        speciesArray.forEach((s: NearbySpecies) => {
            const colorIndex = uniqueSpecies.get(s.id) || 0;
            const color = self.getSpeciesColor(s.id, colorIndex);

            const circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: color,
                color: color,
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map!);

            const distanceText = s.distanceMeters < 1000
                ? `${Math.round(s.distanceMeters)}m away`
                : `${(s.distanceMeters / 1000).toFixed(1)}km away`;

            const popupContent = `
                <div class="nearby-species-popup">
                    <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 4px;">
                        <span style="display: inline-block; width: 12px; height: 12px; border-radius: 50%; background-color: ${color};"></span>
                        <strong>${s.commonName}</strong>
                    </div>
                    <em>${s.scientificName}</em><br/>
                    <span class="distance">${distanceText}</span>
                    ${s.locationDescription ? `<br/><small>${s.locationDescription}</small>` : ''}
                </div>
            `;

            circle.bindPopup(popupContent);
            self.speciesLocationCircles.push(circle);
        });
    },

    clearSpeciesLocationCircles(): void {
        const self = this;
        this.speciesLocationCircles.forEach((circle: L.Circle) => {
            self.map!.removeLayer(circle);
        });
        this.speciesLocationCircles = [];
    },

    getSpeciesColors(): SpeciesColorEntry[] {
        const result: SpeciesColorEntry[] = [];
        this.speciesColorMap.forEach((color: string, id: number) => {
            result.push({ id, color });
        });
        return result;
    },

    resetSpeciesColors(): void {
        this.speciesColorMap.clear();
    },

    initSightingMap(containerId: string, latitude: number, longitude: number): void {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Map container not found:', containerId);
            return;
        }

        // Check if map already exists on this container
        if ((container as any)._leaflet_id) {
            return;
        }

        const savedDarkMode = localStorage.getItem('faunafinder-darkmode');
        const isDarkMode = savedDarkMode !== null
            ? savedDarkMode === 'true'
            : window.matchMedia('(prefers-color-scheme: dark)').matches;

        const tileUrl = isDarkMode
            ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
            : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';

        const attribution = isDarkMode
            ? '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>'
            : '&copy; OpenStreetMap';

        const map = L.map(containerId, {
            center: [latitude, longitude],
            zoom: 14,
            scrollWheelZoom: true,
            dragging: true,
            zoomControl: true
        });

        L.tileLayer(tileUrl, {
            attribution: attribution,
            maxZoom: 18
        }).addTo(map);

        // Add a marker at the sighting location
        const markerIcon = L.divIcon({
            className: 'sighting-location-marker',
            html: '<div style="background-color: #ef4444; width: 16px; height: 16px; border-radius: 50%; border: 3px solid #fff; box-shadow: 0 2px 4px rgba(0,0,0,0.3);"></div>',
            iconSize: [22, 22],
            iconAnchor: [11, 11]
        });

        L.marker([latitude, longitude], { icon: markerIcon }).addTo(map);

        // Add a subtle circle around the marker
        L.circle([latitude, longitude], {
            radius: 50,
            fillColor: '#ef4444',
            color: '#dc2626',
            weight: 2,
            fillOpacity: 0.2
        }).addTo(map);

        // Invalidate size after a short delay to ensure tiles load properly
        setTimeout(() => {
            map.invalidateSize();
        }, 100);
    },

    enableDrawMode(): void {
        if (!this.map || this.isDrawMode) return;

        this.isDrawMode = true;
        const self = this;
        const mapContainer = this.map.getContainer();

        // Add draw mode class for cursor styling
        mapContainer.classList.add('draw-mode');

        // Announce to screen readers
        this.announceToScreenReader('Draw mode enabled. Click and drag on the map to draw a search circle.');

        // Disable map dragging during draw mode
        this.map.dragging.disable();

        const onMouseDown = (e: L.LeafletMouseEvent) => {
            if (!self.isDrawMode) return;

            self.drawStartPoint = e.latlng;

            // Remove existing draw circle if any
            if (self.drawCircle) {
                self.map!.removeLayer(self.drawCircle);
            }

            // Create initial circle with 0 radius
            self.drawCircle = L.circle(e.latlng, {
                radius: 0,
                fillColor: '#8b5cf6',
                color: '#7c3aed',
                weight: 3,
                fillOpacity: 0.2,
                dashArray: '10, 5'
            }).addTo(self.map!);
        };

        const onMouseMove = (e: L.LeafletMouseEvent) => {
            if (!self.isDrawMode || !self.drawStartPoint || !self.drawCircle) return;

            // Calculate radius from start point to current mouse position
            const radius = self.drawStartPoint.distanceTo(e.latlng);
            self.drawCircle.setRadius(radius);
        };

        const onMouseUp = (e: L.LeafletMouseEvent) => {
            if (!self.isDrawMode || !self.drawStartPoint || !self.drawCircle) return;

            const radius = self.drawStartPoint.distanceTo(e.latlng);

            // Minimum radius of 100 meters
            if (radius < 100) {
                self.announceToScreenReader('Circle too small. Please draw a larger area.');
                if (self.drawCircle) {
                    self.map!.removeLayer(self.drawCircle);
                    self.drawCircle = null;
                }
                self.drawStartPoint = null;
                return;
            }

            // Finalize the circle style
            self.drawCircle.setStyle({
                dashArray: undefined,
                fillOpacity: 0.15
            });

            // Save center coordinates BEFORE disabling draw mode (which clears drawStartPoint)
            const center = self.drawStartPoint;

            // Disable draw mode
            self.disableDrawMode();

            // Announce completion
            const radiusKm = radius >= 1000
                ? `${(radius / 1000).toFixed(1)} kilometers`
                : `${Math.round(radius)} meters`;
            self.announceToScreenReader(`Search area drawn with radius of ${radiusKm}. Searching for species.`);

            // Notify Blazor with the drawn circle details
            self.dotNetHelper?.invokeMethodAsync('OnCircleDrawn', center.lat, center.lng, radius);
        };

        // Store handlers for removal later
        (this as any)._drawHandlers = { onMouseDown, onMouseMove, onMouseUp };

        this.map.on('mousedown', onMouseDown);
        this.map.on('mousemove', onMouseMove);
        this.map.on('mouseup', onMouseUp);

        // Track last touch position for touchend (which doesn't have coordinates)
        let lastTouchLatLng: L.LatLng | null = null;

        const getTouchLatLng = (touch: Touch): L.LatLng => {
            const rect = mapContainer.getBoundingClientRect();
            const point = L.point(touch.clientX - rect.left, touch.clientY - rect.top);
            return self.map!.containerPointToLatLng(point);
        };

        const onTouchStart = (e: TouchEvent) => {
            if (!self.isDrawMode) return;
            if (e.touches.length === 1) {
                e.preventDefault();
                const latlng = getTouchLatLng(e.touches[0]);
                lastTouchLatLng = latlng;
                onMouseDown({ latlng } as L.LeafletMouseEvent);
            }
        };

        const onTouchMove = (e: TouchEvent) => {
            if (!self.isDrawMode || !self.drawStartPoint) return;
            if (e.touches.length === 1) {
                e.preventDefault();
                const latlng = getTouchLatLng(e.touches[0]);
                lastTouchLatLng = latlng;
                onMouseMove({ latlng } as L.LeafletMouseEvent);
            }
        };

        const onTouchEnd = (e: TouchEvent) => {
            if (!self.isDrawMode || !self.drawStartPoint || !self.drawCircle) return;
            e.preventDefault();
            // Use last known touch position since touchend doesn't have coordinates
            if (lastTouchLatLng) {
                onMouseUp({ latlng: lastTouchLatLng } as L.LeafletMouseEvent);
            }
            lastTouchLatLng = null;
        };

        // Add touch event listeners directly to the container for better control
        mapContainer.addEventListener('touchstart', onTouchStart, { passive: false });
        mapContainer.addEventListener('touchmove', onTouchMove, { passive: false });
        mapContainer.addEventListener('touchend', onTouchEnd, { passive: false });

        // Store touch handlers for removal
        (this as any)._touchHandlers = { onTouchStart, onTouchMove, onTouchEnd };
    },

    disableDrawMode(): void {
        if (!this.map) return;

        this.isDrawMode = false;
        const mapContainer = this.map.getContainer();

        // Remove draw mode class
        mapContainer.classList.remove('draw-mode');

        // Re-enable map dragging
        this.map.dragging.enable();

        // Remove mouse event handlers
        const handlers = (this as any)._drawHandlers;
        if (handlers) {
            this.map.off('mousedown', handlers.onMouseDown);
            this.map.off('mousemove', handlers.onMouseMove);
            this.map.off('mouseup', handlers.onMouseUp);
            (this as any)._drawHandlers = null;
        }

        // Remove touch event handlers from container
        const touchHandlers = (this as any)._touchHandlers;
        if (touchHandlers) {
            mapContainer.removeEventListener('touchstart', touchHandlers.onTouchStart);
            mapContainer.removeEventListener('touchmove', touchHandlers.onTouchMove);
            mapContainer.removeEventListener('touchend', touchHandlers.onTouchEnd);
            (this as any)._touchHandlers = null;
        }

        this.drawStartPoint = null;
    },

    clearDrawnCircle(): void {
        if (this.drawCircle && this.map) {
            this.map.removeLayer(this.drawCircle);
            this.drawCircle = null;
        }
        this.drawStartPoint = null;
    },

    enablePolygonDrawMode(): void {
        if (!this.map || this.isPolygonDrawMode) return;

        this.isPolygonDrawMode = true;
        const self = this;
        const mapContainer = this.map.getContainer();

        // Add draw mode class for cursor styling
        mapContainer.classList.add('draw-mode', 'polygon-draw-mode');

        // Announce to screen readers
        this.announceToScreenReader('Polygon draw mode enabled. Click on the map to add vertices. Double-click or click the first point to complete the shape.');

        // Disable map dragging during draw mode
        this.map.dragging.disable();

        // Clear any existing polygon drawing state
        this.clearDrawnPolygon();
        this.drawPolygonPoints = [];

        const onClick = (e: L.LeafletMouseEvent) => {
            if (!self.isPolygonDrawMode) return;

            const point = e.latlng;

            // Check if clicking near the first point to close the polygon
            if (self.drawPolygonPoints.length >= 3) {
                const firstPoint = self.drawPolygonPoints[0];
                const distance = self.map!.latLngToContainerPoint(point).distanceTo(
                    self.map!.latLngToContainerPoint(firstPoint)
                );

                // If within 15 pixels of the first point, close the polygon
                if (distance < 15) {
                    self.finishPolygon();
                    return;
                }
            }

            // Add point to the polygon
            self.drawPolygonPoints.push(point);

            // Add a marker for this vertex
            const marker = L.circleMarker(point, {
                radius: 6,
                fillColor: self.drawPolygonPoints.length === 1 ? '#22c55e' : '#8b5cf6',
                color: '#fff',
                weight: 2,
                fillOpacity: 1
            }).addTo(self.map!);
            self.drawPolygonMarkers.push(marker);

            // Update the polygon
            self.updateDrawingPolygon();
        };

        const onMouseMove = (e: L.LeafletMouseEvent) => {
            if (!self.isPolygonDrawMode || self.drawPolygonPoints.length === 0) return;

            // Update preview line from last point to current mouse position
            const lastPoint = self.drawPolygonPoints[self.drawPolygonPoints.length - 1];

            if (self.drawPolygonPreviewLine) {
                self.map!.removeLayer(self.drawPolygonPreviewLine);
            }

            self.drawPolygonPreviewLine = L.polyline([lastPoint, e.latlng], {
                color: '#8b5cf6',
                weight: 2,
                dashArray: '5, 5',
                opacity: 0.7
            }).addTo(self.map!);
        };

        const onDblClick = (e: L.LeafletMouseEvent) => {
            if (!self.isPolygonDrawMode) return;

            L.DomEvent.stopPropagation(e.originalEvent);
            L.DomEvent.preventDefault(e.originalEvent);

            if (self.drawPolygonPoints.length >= 3) {
                self.finishPolygon();
            } else {
                self.announceToScreenReader('At least 3 points are required to create a polygon.');
            }
        };

        const onKeyDown = (e: KeyboardEvent) => {
            if (!self.isPolygonDrawMode) return;

            if (e.key === 'Escape') {
                // Cancel polygon drawing
                self.clearDrawnPolygon();
                self.disablePolygonDrawMode();
                self.announceToScreenReader('Polygon drawing cancelled.');
            } else if (e.key === 'Enter' && self.drawPolygonPoints.length >= 3) {
                // Complete polygon on Enter
                self.finishPolygon();
            } else if ((e.key === 'Backspace' || e.key === 'Delete') && self.drawPolygonPoints.length > 0) {
                // Remove last point
                self.drawPolygonPoints.pop();
                const lastMarker = self.drawPolygonMarkers.pop();
                if (lastMarker) {
                    self.map!.removeLayer(lastMarker);
                }
                self.updateDrawingPolygon();
                self.announceToScreenReader(`Point removed. ${self.drawPolygonPoints.length} points remaining.`);
            }
        };

        // Store handlers for removal later
        (this as any)._polygonDrawHandlers = { onClick, onMouseMove, onDblClick, onKeyDown };

        this.map.on('click', onClick);
        this.map.on('mousemove', onMouseMove);
        this.map.on('dblclick', onDblClick);
        document.addEventListener('keydown', onKeyDown);

        // Disable double-click zoom during polygon drawing
        this.map.doubleClickZoom.disable();
    },

    updateDrawingPolygon(): void {
        if (!this.map) return;

        // Remove existing polygon
        if (this.drawPolygon) {
            this.map.removeLayer(this.drawPolygon);
            this.drawPolygon = null;
        }

        // Need at least 2 points to draw a line/polygon preview
        if (this.drawPolygonPoints.length < 2) return;

        // Draw the polygon (or line if only 2 points)
        this.drawPolygon = L.polygon(this.drawPolygonPoints, {
            fillColor: '#8b5cf6',
            color: '#7c3aed',
            weight: 3,
            fillOpacity: 0.2,
            dashArray: '10, 5'
        }).addTo(this.map);
    },

    finishPolygon(): void {
        if (!this.map || this.drawPolygonPoints.length < 3) return;

        const self = this;

        // Remove preview line
        if (this.drawPolygonPreviewLine) {
            this.map.removeLayer(this.drawPolygonPreviewLine);
            this.drawPolygonPreviewLine = null;
        }

        // Remove vertex markers
        this.drawPolygonMarkers.forEach(marker => {
            self.map!.removeLayer(marker);
        });
        this.drawPolygonMarkers = [];

        // Update polygon style to final
        if (this.drawPolygon) {
            this.drawPolygon.setStyle({
                dashArray: undefined,
                fillOpacity: 0.15
            });
        }

        // Get the polygon coordinates
        const coordinates = this.drawPolygonPoints.map(p => ({
            latitude: p.lat,
            longitude: p.lng
        }));

        // Disable polygon draw mode
        this.disablePolygonDrawMode();

        // Announce completion
        this.announceToScreenReader(`Polygon drawn with ${coordinates.length} vertices. Searching for species in the area.`);

        // Notify Blazor with the drawn polygon details
        this.dotNetHelper?.invokeMethodAsync('OnPolygonDrawn', coordinates);
    },

    disablePolygonDrawMode(): void {
        if (!this.map) return;

        this.isPolygonDrawMode = false;
        const mapContainer = this.map.getContainer();

        // Remove draw mode classes
        mapContainer.classList.remove('draw-mode', 'polygon-draw-mode');

        // Re-enable map dragging and double-click zoom
        this.map.dragging.enable();
        this.map.doubleClickZoom.enable();

        // Remove preview line
        if (this.drawPolygonPreviewLine) {
            this.map.removeLayer(this.drawPolygonPreviewLine);
            this.drawPolygonPreviewLine = null;
        }

        // Remove event handlers
        const handlers = (this as any)._polygonDrawHandlers;
        if (handlers) {
            this.map.off('click', handlers.onClick);
            this.map.off('mousemove', handlers.onMouseMove);
            this.map.off('dblclick', handlers.onDblClick);
            document.removeEventListener('keydown', handlers.onKeyDown);
            (this as any)._polygonDrawHandlers = null;
        }
    },

    clearDrawnPolygon(): void {
        if (!this.map) return;

        // Remove polygon
        if (this.drawPolygon) {
            this.map.removeLayer(this.drawPolygon);
            this.drawPolygon = null;
        }

        // Remove preview line
        if (this.drawPolygonPreviewLine) {
            this.map.removeLayer(this.drawPolygonPreviewLine);
            this.drawPolygonPreviewLine = null;
        }

        // Remove vertex markers
        const self = this;
        this.drawPolygonMarkers.forEach(marker => {
            self.map!.removeLayer(marker);
        });
        this.drawPolygonMarkers = [];

        // Clear points
        this.drawPolygonPoints = [];
    }
};

// ============================================================================
// Download File Function
// ============================================================================

/**
 * Download a file from base64 content
 */
function downloadFile(base64: string, fileName: string, contentType: string): void {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array<number>(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });

    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
}

window.downloadFile = downloadFile;
