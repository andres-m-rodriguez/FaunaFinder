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

        // Handle resize
        window.addEventListener('resize', () => {
            this.isMobile = window.innerWidth < 640;
        });
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
