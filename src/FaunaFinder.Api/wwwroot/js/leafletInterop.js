window.leafletInterop = {
    map: null,
    geojsonLayer: null,
    dotNetHelper: null,

    initMap: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.map = L.map('map').setView([18.2208, -66.5901], 9);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(this.map);

        this.loadGeoJson();
    },

    loadGeoJson: function () {
        fetch('/data/pr-municipalities.geojson')
            .then(r => {
                if (!r.ok) throw new Error('HTTP ' + r.status + ': ' + r.statusText);
                return r.json();
            })
            .then(data => {
                this.geojsonLayer = L.geoJSON(data, {
                    style: () => ({ fillColor: '#40916c', weight: 2, opacity: 1, color: '#1b4332', fillOpacity: 0.4 }),
                    onEachFeature: (feature, layer) => {
                        const name = feature.properties.NAME || 'Unknown';
                        const id = feature.properties.GEOID || name;
                        layer.bindTooltip(name, { direction: 'center' });
                        layer.on({
                            mouseover: e => e.target.setStyle({ fillColor: '#74c69d', weight: 3, fillOpacity: 0.7 }),
                            mouseout: e => this.geojsonLayer.resetStyle(e.target),
                            click: () => this.dotNetHelper?.invokeMethodAsync('OnMunicipalityClick', id)
                        });
                    }
                }).addTo(this.map);
                console.log('GeoJSON loaded successfully with', data.features.length, 'features');
            })
            .catch(err => {
                console.error('GeoJSON load error:', err);
                L.marker([18.2208, -66.5901]).addTo(this.map).bindPopup('GeoJSON error: ' + err.message).openPopup();
            });
    }
};
