window.leafletInterop = {
    map: null,
    geojsonLayer: null,
    dotNetHelper: null,
    isMobile: false,

    initMap: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.isMobile = window.innerWidth < 640;

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

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap',
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

    loadGeoJson: function () {
        fetch('/data/pr-municipios.geojson')
            .then(r => {
                if (!r.ok) throw new Error('HTTP ' + r.status + ': ' + r.statusText);
                return r.json();
            })
            .then(data => {
                this.geojsonLayer = L.geoJSON(data, {
                    style: this.defaultStyle,
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

    defaultStyle: function () {
        return {
            fillColor: '#e5e7eb',
            weight: 1,
            color: '#9ca3af',
            fillOpacity: 0.3
        };
    },

    highlightFeature: function (e) {
        e.target.setStyle({
            fillColor: '#40916c',
            weight: 2,
            color: '#1b4332',
            fillOpacity: 0.6
        });
    },

    resetHighlight: function (e) {
        this.geojsonLayer.resetStyle(e.target);
    },

    highlightMunicipality: function (county) {
        if (!this.geojsonLayer) return;
        this.geojsonLayer.eachLayer(layer => {
            if (layer.feature.properties.COUNTY === county) {
                layer.setStyle({
                    fillColor: '#40916c',
                    weight: 2,
                    color: '#1b4332',
                    fillOpacity: 0.6
                });
            } else {
                this.geojsonLayer.resetStyle(layer);
            }
        });
    }
};
