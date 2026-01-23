/**
 * Leaflet Interop for FaunaFinder Blazor Application
 * Provides map functionality with TypeScript type safety
 */

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

    // Tile layer URLs
    lightTileUrl: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
    darkTileUrl: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',

    // Theme colors
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

    initMap: function (dotNetHelper: DotNetObjectReference): void {
        this.dotNetHelper = dotNetHelper;
        this.isMobile = window.innerWidth < 640;

        // Check initial dark mode state from localStorage or system preference
        const savedDarkMode = localStorage.getItem('faunafinder-darkmode');
        if (savedDarkMode !== null) {
            this.isDarkMode = savedDarkMode === 'true';
        } else {
            // Fall back to system preference
            this.isDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
        }

        // Puerto Rico bounds
        const PR_BOUNDS: L.LatLngBoundsExpression = [
            [17.1176, -67.9426],  // SW
            [19.42, -64.9007]     // NE
        ];

        // Responsive zoom
        const zoom = this.isMobile ? 8 : 9;

        this.map = L.map('map', {
            center: [18.15, -66.5],
            zoom: zoom,
            maxBounds: PR_BOUNDS,
            maxBoundsViscosity: 1.0,
            scrollWheelZoom: !this.isMobile
        });

        // Add initial tile layer based on dark mode state
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

        this.loadGeoJson();

        // Add locate control
        this.createLocateControl();

        // Handle resize
        window.addEventListener('resize', () => {
            this.isMobile = window.innerWidth < 640;
        });
    },

    createLocateControl: function (): void {
        const self = this;

        // Create custom locate control
        const LocateControl = L.Control.extend({
            options: {
                position: 'topleft'
            },

            onAdd: function (): HTMLElement {
                const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-locate');
                const button = L.DomUtil.create('a', 'leaflet-control-locate-button', container);
                button.href = '#';
                button.title = 'Locate me';
                button.setAttribute('role', 'button');
                button.setAttribute('aria-label', 'Locate me');
                button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm8.94 3A8.994 8.994 0 0 0 13 3.06V1h-2v2.06A8.994 8.994 0 0 0 3.06 11H1v2h2.06A8.994 8.994 0 0 0 11 20.94V23h2v-2.06A8.994 8.994 0 0 0 20.94 13H23v-2h-2.06zM12 19c-3.87 0-7-3.13-7-7s3.13-7 7-7 7 3.13 7 7-3.13 7-7 7z"/></svg>';

                L.DomEvent.disableClickPropagation(container);
                L.DomEvent.on(button, 'click', function (e: Event) {
                    L.DomEvent.preventDefault(e);
                    self.locateUser();
                });

                self.locateControl = container;
                return container;
            }
        });

        new LocateControl().addTo(this.map!);
    },

    locateUser: function (): void {
        if (this.isLocating) return;

        const self = this;

        if (!navigator.geolocation) {
            self.showLocationError('geolocation_unsupported');
            return;
        }

        this.isLocating = true;
        this.setLocateControlState('loading');

        navigator.geolocation.getCurrentPosition(
            function (position: GeolocationPosition) {
                self.isLocating = false;
                self.setLocateControlState('default');

                const lat = position.coords.latitude;
                const lng = position.coords.longitude;
                const accuracy = position.coords.accuracy;

                // Store user location
                self.userLocation = { latitude: lat, longitude: lng };

                // Remove existing user location marker if present
                if (self.userLocationMarker) {
                    self.map!.removeLayer(self.userLocationMarker);
                }

                // Create custom icon for user location
                const userIcon = L.divIcon({
                    className: 'user-location-marker',
                    html: '<div class="user-location-pulse"></div><div class="user-location-dot"></div>',
                    iconSize: [24, 24],
                    iconAnchor: [12, 12]
                });

                // Add marker at user location
                self.userLocationMarker = L.marker([lat, lng], { icon: userIcon })
                    .addTo(self.map!)
                    .bindPopup('You are here');

                // Fly to user location with animation
                self.map!.flyTo([lat, lng], 14, {
                    duration: 1.5
                });

                // Notify Blazor that user location is available
                self.dotNetHelper?.invokeMethodAsync('OnUserLocationFound', lat, lng);
            },
            function (error: GeolocationPositionError) {
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

    setLocateControlState: function (state: string): void {
        if (!this.locateControl) return;

        const button = this.locateControl.querySelector('.leaflet-control-locate-button');
        if (!button) return;

        if (state === 'loading') {
            button.classList.add('loading');
            button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor" class="spin"><path d="M12 4V2A10 10 0 0 0 2 12h2a8 8 0 0 1 8-8z"/></svg>';
        } else {
            button.classList.remove('loading');
            button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm8.94 3A8.994 8.994 0 0 0 13 3.06V1h-2v2.06A8.994 8.994 0 0 0 3.06 11H1v2h2.06A8.994 8.994 0 0 0 11 20.94V23h2v-2.06A8.994 8.994 0 0 0 20.94 13H23v-2h-2.06zM12 19c-3.87 0-7-3.13-7-7s3.13-7 7-7 7 3.13 7 7-3.13 7-7 7z"/></svg>';
        }
    },

    showLocationError: function (errorType: string): void {
        const messages: { [key: string]: string } = {
            'geolocation_unsupported': 'Geolocation is not supported by your browser.',
            'permission_denied': 'Location access was denied. Please enable location permissions.',
            'position_unavailable': 'Unable to determine your location.',
            'timeout': 'Location request timed out. Please try again.',
            'unknown': 'An unknown error occurred while getting your location.'
        };

        const message = messages[errorType] || messages['unknown'];

        // Show error popup at map center
        if (this.map) {
            L.popup()
                .setLatLng(this.map.getCenter())
                .setContent('<div class="location-error-popup"><strong>Location Error</strong><br/>' + message + '</div>')
                .openOn(this.map);
        }
    },

    setDarkMode: function (isDark: boolean): void {
        this.isDarkMode = isDark;

        // Update tile layer if map exists
        if (this.map) {
            if (this.tileLayer) {
                this.map.removeLayer(this.tileLayer);
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

            // Move tile layer to back so GeoJSON stays on top
            this.tileLayer.bringToBack();
        }

        // Update GeoJSON layer styles
        if (this.geojsonLayer) {
            this.geojsonLayer.setStyle(this.getDefaultStyle());
        }
    },

    loadGeoJson: function (): void {
        const self = this;
        fetch('/data/pr-municipios.geojson')
            .then(r => {
                if (!r.ok) throw new Error('HTTP ' + r.status + ': ' + r.statusText);
                return r.json();
            })
            .then((data: GeoJSON.FeatureCollection) => {
                self.geojsonLayer = L.geoJSON(data, {
                    style: () => self.getDefaultStyle(),
                    onEachFeature: (feature, layer) => {
                        const props = feature.properties as MunicipalityProperties;
                        const name = props.NAME;
                        const state = props.STATE;
                        const county = props.COUNTY;
                        // Combine STATE + COUNTY to form the full GeoJsonId (e.g., "72" + "071" = "72071")
                        const geoJsonId = state + county;

                        layer.bindTooltip(name, {
                            direction: 'center',
                            className: 'municipality-tooltip'
                        });

                        layer.on({
                            mouseover: (e: L.LeafletMouseEvent) => self.highlightFeature(e),
                            mouseout: (e: L.LeafletMouseEvent) => self.resetHighlight(e),
                            click: () => self.dotNetHelper?.invokeMethodAsync('OnMunicipalityClick', geoJsonId, name)
                        });
                    }
                }).addTo(self.map!);
                console.log('GeoJSON loaded successfully with', data.features.length, 'features');
            })
            .catch((err: Error) => {
                console.error('GeoJSON load error:', err);
                if (self.map) {
                    L.marker([18.15, -66.5]).addTo(self.map).bindPopup('GeoJSON error: ' + err.message).openPopup();
                }
            });
    },

    getDefaultStyle: function (): L.PathOptions {
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        return {
            fillColor: theme.fillColor,
            weight: 1,
            color: theme.borderColor,
            fillOpacity: 0.3
        };
    },

    defaultStyle: function (): L.PathOptions {
        return this.getDefaultStyle();
    },

    highlightFeature: function (e: L.LeafletMouseEvent): void {
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        const layer = e.target as L.Path;
        layer.setStyle({
            fillColor: theme.highlightFill,
            weight: 2,
            color: theme.highlightBorder,
            fillOpacity: 0.6
        });
    },

    resetHighlight: function (e: L.LeafletMouseEvent): void {
        if (this.geojsonLayer) {
            this.geojsonLayer.resetStyle(e.target as L.Path);
        }
    },

    highlightMunicipality: function (county: string): void {
        if (!this.geojsonLayer) return;
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        const self = this;
        let targetLayer: L.Layer | null = null;

        this.geojsonLayer.eachLayer((layer: L.Layer) => {
            const geoLayer = layer as L.GeoJSON;
            const feature = (geoLayer as unknown as { feature: GeoJSON.Feature }).feature;
            if (feature && feature.properties) {
                const props = feature.properties as MunicipalityProperties;
                if (props.COUNTY === county) {
                    // Selected style - prominent dashed border
                    (layer as L.Path).setStyle({
                        fillColor: theme.highlightFill,
                        weight: 4,
                        color: '#2563eb',
                        fillOpacity: 0.5,
                        dashArray: '10, 6',
                        dashOffset: '0'
                    });
                    targetLayer = layer;
                } else {
                    self.geojsonLayer!.resetStyle(layer as L.Path);
                }
            }
        });

        // Zoom to the selected municipality
        if (targetLayer && this.map) {
            const bounds = (targetLayer as L.Polygon).getBounds();
            this.map.fitBounds(bounds, { padding: [50, 50], maxZoom: 12 });
        }
    },

    showSpeciesLocations: function (speciesName: string, locations: SpeciesLocation[]): void {
        // Clear existing location circles
        this.clearSpeciesLocations();

        if (!locations || locations.length === 0) return;

        const self = this;
        // Create circles for each location
        locations.forEach(loc => {
            const circle = L.circle([loc.latitude, loc.longitude], {
                radius: loc.radiusMeters,
                fillColor: '#ef4444',
                color: '#dc2626',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map!);

            // Add popup with species name and description
            const popupContent = loc.description
                ? `<strong>${speciesName}</strong><br/>${loc.description}`
                : `<strong>${speciesName}</strong>`;
            circle.bindPopup(popupContent);

            self.locationCircles.push(circle);
        });

        // Fit map bounds to show all circles
        if (this.locationCircles.length > 0) {
            const group = L.featureGroup(this.locationCircles);
            this.map!.fitBounds(group.getBounds(), { padding: [50, 50] });
        }
    },

    clearSpeciesLocations: function (): void {
        const self = this;
        this.locationCircles.forEach((circle: L.Circle) => {
            self.map!.removeLayer(circle);
        });
        this.locationCircles = [];
    },

    focusOnLocation: function (index: number): void {
        if (index >= 0 && index < this.locationCircles.length) {
            const circle = this.locationCircles[index];
            this.map!.fitBounds(circle.getBounds(), { padding: [50, 50], maxZoom: 14 });
            circle.openPopup();
        }
    },

    focusAllLocations: function (): void {
        if (this.locationCircles.length > 0) {
            const group = L.featureGroup(this.locationCircles);
            this.map!.fitBounds(group.getBounds(), { padding: [50, 50] });
            // Close any open popups
            this.locationCircles.forEach((circle: L.Circle) => circle.closePopup());
        }
    },

    // Species Near Me Functions

    showSearchRadius: function (radiusMeters: number): void {
        if (!this.userLocation || !this.map) return;

        // Remove existing search radius circle
        if (this.searchRadiusCircle) {
            this.map.removeLayer(this.searchRadiusCircle);
        }

        // Create search radius circle
        this.searchRadiusCircle = L.circle(
            [this.userLocation.latitude, this.userLocation.longitude],
            {
                radius: radiusMeters,
                fillColor: '#3b82f6',
                color: '#2563eb',
                weight: 2,
                fillOpacity: 0.1,
                dashArray: '5, 5'
            }
        ).addTo(this.map);

        // Fit map to show the entire search radius
        this.map.fitBounds(this.searchRadiusCircle.getBounds(), { padding: [50, 50] });
    },

    clearSearchRadius: function (): void {
        if (this.searchRadiusCircle && this.map) {
            this.map.removeLayer(this.searchRadiusCircle);
            this.searchRadiusCircle = null;
        }
        this.clearNearbySpeciesMarkers();
    },

    showNearbySpecies: function (species: NearbySpeciesLocation[]): void {
        // Clear existing markers
        this.clearNearbySpeciesMarkers();

        // Ensure species is an array
        const speciesArray = Array.isArray(species) ? species : [];

        if (speciesArray.length === 0 || !this.map) return;

        const self = this;

        speciesArray.forEach((s, index) => {
            // Create circle for species location
            const circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: '#10b981',
                color: '#059669',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map!);

            // Create popup content
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

    clearNearbySpeciesMarkers: function (): void {
        const self = this;
        this.nearbySpeciesMarkers.forEach((marker: L.Circle) => {
            self.map!.removeLayer(marker);
        });
        this.nearbySpeciesMarkers = [];
    },

    getUserLocation: function (): { latitude: number; longitude: number } | null {
        return this.userLocation;
    },

    focusOnNearbySpecies: function (index: number): void {
        if (index >= 0 && index < this.nearbySpeciesMarkers.length) {
            const circle = this.nearbySpeciesMarkers[index];
            this.map!.fitBounds(circle.getBounds(), { padding: [50, 50], maxZoom: 14 });
            circle.openPopup();
        }
    },

    // Species location circles with unique colors
    speciesColorPalette: [
        '#e63946', // Red
        '#f4a261', // Orange
        '#2a9d8f', // Teal
        '#e9c46a', // Yellow
        '#264653', // Dark blue
        '#9b5de5', // Purple
        '#00bbf9', // Sky blue
        '#f15bb5', // Pink
        '#00f5d4', // Cyan
        '#fee440', // Bright yellow
        '#8338ec', // Violet
        '#fb5607', // Bright orange
        '#3a86ff', // Blue
        '#ff006e', // Magenta
        '#06d6a0', // Mint
    ],

    getSpeciesColor: function (speciesId: number, index: number): string {
        // Check if we already assigned a color to this species
        if (this.speciesColorMap.has(speciesId)) {
            return this.speciesColorMap.get(speciesId)!;
        }

        // Assign a new color from the palette
        const color = this.speciesColorPalette[index % this.speciesColorPalette.length];
        this.speciesColorMap.set(speciesId, color);
        return color;
    },

    showSpeciesLocationCircles: function (species: NearbySpeciesLocation[]): void {
        // Clear existing species location circles
        this.clearSpeciesLocationCircles();

        // Ensure species is an array
        const speciesArray = Array.isArray(species) ? species : [];

        if (speciesArray.length === 0 || !this.map) return;

        const self = this;

        // Group species by ID to get unique species and assign colors
        const uniqueSpecies = new Map<number, number>();
        speciesArray.forEach((s, idx) => {
            if (!uniqueSpecies.has(s.id)) {
                uniqueSpecies.set(s.id, uniqueSpecies.size);
            }
        });

        speciesArray.forEach((s) => {
            const colorIndex = uniqueSpecies.get(s.id) || 0;
            const color = self.getSpeciesColor(s.id, colorIndex);

            // Create circle for species location
            const circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: color,
                color: color,
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map!);

            // Create popup content
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

    clearSpeciesLocationCircles: function (): void {
        const self = this;
        this.speciesLocationCircles.forEach((circle: L.Circle) => {
            self.map!.removeLayer(circle);
        });
        this.speciesLocationCircles = [];
    },

    getSpeciesColors: function (): { id: number; color: string }[] {
        const result: { id: number; color: string }[] = [];
        this.speciesColorMap.forEach((color, id) => {
            result.push({ id, color });
        });
        return result;
    },

    resetSpeciesColors: function (): void {
        this.speciesColorMap.clear();
    }
};

/**
 * Download file utility function for Blazor interop
 * Converts base64 data to a blob and triggers a download
 */
function downloadFile(base64: string, fileName: string, contentType: string): void {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
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

// Expose downloadFile to window for Blazor interop
(window as Window & { downloadFile: typeof downloadFile }).downloadFile = downloadFile;
