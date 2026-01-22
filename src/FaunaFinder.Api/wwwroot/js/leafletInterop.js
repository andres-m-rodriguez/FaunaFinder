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
        fillColor: '#e5e7eb',
        borderColor: '#9ca3af',
        highlightFill: '#40916c',
        highlightBorder: '#1b4332'
    },
    darkTheme: {
        fillColor: '#9ca3af',
        borderColor: '#d1d5db',
        highlightFill: '#52b788',
        highlightBorder: '#86efac'
    },

    initMap: function (dotNetHelper) {
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

        // Puerto Rico bounds (from hayluz)
        const PR_BOUNDS = [
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

    setDarkMode: function (isDark) {
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

    loadGeoJson: function () {
        fetch('/data/pr-municipios.geojson')
            .then(r => {
                if (!r.ok) throw new Error('HTTP ' + r.status + ': ' + r.statusText);
                return r.json();
            })
            .then(data => {
                this.geojsonLayer = L.geoJSON(data, {
                    style: () => this.getDefaultStyle(),
                    onEachFeature: (feature, layer) => {
                        const name = feature.properties.NAME;
                        const county = feature.properties.COUNTY;

                        layer.bindTooltip(name, {
                            direction: 'center',
                            className: 'municipality-tooltip'
                        });

                        layer.on({
                            mouseover: e => this.highlightFeature(e),
                            mouseout: e => this.resetHighlight(e),
                            click: () => this.dotNetHelper?.invokeMethodAsync('OnMunicipalityClick', county, name)
                        });
                    }
                }).addTo(this.map);
                console.log('GeoJSON loaded successfully with', data.features.length, 'features');
            })
            .catch(err => {
                console.error('GeoJSON load error:', err);
                L.marker([18.15, -66.5]).addTo(this.map).bindPopup('GeoJSON error: ' + err.message).openPopup();
            });
    },

    getDefaultStyle: function () {
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        return {
            fillColor: theme.fillColor,
            weight: 1,
            color: theme.borderColor,
            fillOpacity: 0.3
        };
    },

    defaultStyle: function () {
        return this.getDefaultStyle();
    },

    highlightFeature: function (e) {
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        e.target.setStyle({
            fillColor: theme.highlightFill,
            weight: 2,
            color: theme.highlightBorder,
            fillOpacity: 0.6
        });
    },

    resetHighlight: function (e) {
        this.geojsonLayer.resetStyle(e.target);
    },

    highlightMunicipality: function (county) {
        if (!this.geojsonLayer) return;
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        this.geojsonLayer.eachLayer(layer => {
            if (layer.feature.properties.COUNTY === county) {
                layer.setStyle({
                    fillColor: theme.highlightFill,
                    weight: 2,
                    color: theme.highlightBorder,
                    fillOpacity: 0.6
                });
            } else {
                this.geojsonLayer.resetStyle(layer);
            }
        });
    },

    showSpeciesLocations: function (speciesName, locations) {
        // Clear existing location circles
        this.clearSpeciesLocations();

        if (!locations || locations.length === 0) return;

        // Create circles for each location
        locations.forEach(loc => {
            const circle = L.circle([loc.latitude, loc.longitude], {
                radius: loc.radiusMeters,
                fillColor: '#ef4444',
                color: '#dc2626',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(this.map);

            // Add popup with species name and description
            const popupContent = loc.description
                ? `<strong>${speciesName}</strong><br/>${loc.description}`
                : `<strong>${speciesName}</strong>`;
            circle.bindPopup(popupContent);

            this.locationCircles.push(circle);
        });

        // Fit map bounds to show all circles
        if (this.locationCircles.length > 0) {
            const group = L.featureGroup(this.locationCircles);
            this.map.fitBounds(group.getBounds(), { padding: [50, 50] });
        }
    },

    clearSpeciesLocations: function () {
        this.locationCircles.forEach(circle => {
            this.map.removeLayer(circle);
        });
        this.locationCircles = [];
    },

    focusOnLocation: function (index) {
        if (index >= 0 && index < this.locationCircles.length) {
            const circle = this.locationCircles[index];
            this.map.fitBounds(circle.getBounds(), { padding: [50, 50], maxZoom: 14 });
            circle.openPopup();
        }
    },

    focusAllLocations: function () {
        if (this.locationCircles.length > 0) {
            const group = L.featureGroup(this.locationCircles);
            this.map.fitBounds(group.getBounds(), { padding: [50, 50] });
            // Close any open popups
            this.locationCircles.forEach(circle => circle.closePopup());
        }
    }
};
