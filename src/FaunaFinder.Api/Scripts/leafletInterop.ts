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
                        const county = props.COUNTY;

                        layer.bindTooltip(name, {
                            direction: 'center',
                            className: 'municipality-tooltip'
                        });

                        layer.on({
                            mouseover: (e: L.LeafletMouseEvent) => self.highlightFeature(e),
                            mouseout: (e: L.LeafletMouseEvent) => self.resetHighlight(e),
                            click: () => self.dotNetHelper?.invokeMethodAsync('OnMunicipalityClick', county, name)
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
        this.geojsonLayer.eachLayer((layer: L.Layer) => {
            const geoLayer = layer as L.GeoJSON;
            const feature = (geoLayer as unknown as { feature: GeoJSON.Feature }).feature;
            if (feature && feature.properties) {
                const props = feature.properties as MunicipalityProperties;
                if (props.COUNTY === county) {
                    (layer as L.Path).setStyle({
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
    }
};
