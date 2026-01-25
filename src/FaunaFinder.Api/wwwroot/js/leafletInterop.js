"use strict";
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
    speciesColorMap: new Map(),
    apiBaseUrl: '',
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
    setApiBaseUrl: function (url) {
        this.apiBaseUrl = url.replace(/\/$/, '');
    },
    initMap: function (dotNetHelper, apiBaseUrl) {
        this.dotNetHelper = dotNetHelper;
        if (apiBaseUrl) {
            this.apiBaseUrl = apiBaseUrl.replace(/\/$/, '');
        }
        this.isMobile = window.innerWidth < 640;
        const savedDarkMode = localStorage.getItem('faunafinder-darkmode');
        if (savedDarkMode !== null) {
            this.isDarkMode = savedDarkMode === 'true';
        }
        else {
            this.isDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
        }
        const PR_BOUNDS = [
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
        this.createLocateControl();
        window.addEventListener('resize', () => {
            this.isMobile = window.innerWidth < 640;
        });
    },
    createLocateControl: function () {
        const self = this;
        const LocateControl = L.Control.extend({
            options: {
                position: 'topleft'
            },
            onAdd: function () {
                const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-locate');
                const button = L.DomUtil.create('a', 'leaflet-control-locate-button', container);
                button.href = '#';
                button.title = 'Locate me';
                button.setAttribute('role', 'button');
                button.setAttribute('aria-label', 'Locate me');
                button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm8.94 3A8.994 8.994 0 0 0 13 3.06V1h-2v2.06A8.994 8.994 0 0 0 3.06 11H1v2h2.06A8.994 8.994 0 0 0 11 20.94V23h2v-2.06A8.994 8.994 0 0 0 20.94 13H23v-2h-2.06zM12 19c-3.87 0-7-3.13-7-7s3.13-7 7-7 7 3.13 7 7-3.13 7-7 7z"/></svg>';
                L.DomEvent.disableClickPropagation(container);
                L.DomEvent.on(button, 'click', function (e) {
                    L.DomEvent.preventDefault(e);
                    self.locateUser();
                });
                self.locateControl = container;
                return container;
            }
        });
        new LocateControl().addTo(this.map);
    },
    locateUser: function () {
        if (this.isLocating)
            return;
        const self = this;
        if (!navigator.geolocation) {
            self.showLocationError('geolocation_unsupported');
            return;
        }
        this.isLocating = true;
        this.setLocateControlState('loading');
        navigator.geolocation.getCurrentPosition(function (position) {
            self.isLocating = false;
            self.setLocateControlState('default');
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;
            const accuracy = position.coords.accuracy;
            self.userLocation = { latitude: lat, longitude: lng };
            if (self.userLocationMarker) {
                self.map.removeLayer(self.userLocationMarker);
            }
            const userIcon = L.divIcon({
                className: 'user-location-marker',
                html: '<div class="user-location-pulse"></div><div class="user-location-dot"></div>',
                iconSize: [24, 24],
                iconAnchor: [12, 12]
            });
            self.userLocationMarker = L.marker([lat, lng], { icon: userIcon })
                .addTo(self.map)
                .bindPopup('You are here');
            self.map.flyTo([lat, lng], 14, {
                duration: 1.5
            });
            self.dotNetHelper?.invokeMethodAsync('OnUserLocationFound', lat, lng);
        }, function (error) {
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
        }, {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 60000
        });
    },
    setLocateControlState: function (state) {
        if (!this.locateControl)
            return;
        const button = this.locateControl.querySelector('.leaflet-control-locate-button');
        if (!button)
            return;
        if (state === 'loading') {
            button.classList.add('loading');
            button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor" class="spin"><path d="M12 4V2A10 10 0 0 0 2 12h2a8 8 0 0 1 8-8z"/></svg>';
        }
        else {
            button.classList.remove('loading');
            button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm8.94 3A8.994 8.994 0 0 0 13 3.06V1h-2v2.06A8.994 8.994 0 0 0 3.06 11H1v2h2.06A8.994 8.994 0 0 0 11 20.94V23h2v-2.06A8.994 8.994 0 0 0 20.94 13H23v-2h-2.06zM12 19c-3.87 0-7-3.13-7-7s3.13-7 7-7 7 3.13 7 7-3.13 7-7 7z"/></svg>';
        }
    },
    showLocationError: function (errorType) {
        const messages = {
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
    setDarkMode: function (isDark) {
        this.isDarkMode = isDark;
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
            this.tileLayer.bringToBack();
        }
        if (this.geojsonLayer) {
            this.geojsonLayer.setStyle(this.getDefaultStyle());
        }
    },
    loadGeoJson: function () {
        const self = this;
        const CACHE_KEY = 'pr-municipios-geojson';
        const VERSION_KEY = 'pr-municipios-version';
        const CURRENT_VERSION = 'v1';

        const processGeoJson = (data) => {
            self.geojsonLayer = L.geoJSON(data, {
                style: () => self.getDefaultStyle(),
                onEachFeature: (feature, layer) => {
                    const props = feature.properties;
                    const name = props.NAME;
                    const state = props.STATE;
                    const county = props.COUNTY;
                    const geoJsonId = state + county;
                    layer.bindTooltip(name, {
                        direction: 'center',
                        className: 'municipality-tooltip'
                    });
                    layer.on({
                        mouseover: (e) => self.highlightFeature(e),
                        mouseout: (e) => self.resetHighlight(e),
                        click: () => self.dotNetHelper?.invokeMethodAsync('OnMunicipalityClick', geoJsonId, name)
                    });
                }
            }).addTo(self.map);
            console.log('GeoJSON loaded successfully with', data.features.length, 'features');
        };

        // Check localStorage cache first
        const cached = localStorage.getItem(CACHE_KEY);
        const cacheVersion = localStorage.getItem(VERSION_KEY);

        if (cached && cacheVersion === CURRENT_VERSION) {
            try {
                const data = JSON.parse(cached);
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
            .then(r => {
                if (!r.ok)
                    throw new Error('API returned ' + r.status);
                return r.json();
            })
            .then((data) => {
                if (data.features && data.features.length > 0) {
                    // Store in localStorage for future use
                    try {
                        localStorage.setItem(CACHE_KEY, JSON.stringify(data));
                        localStorage.setItem(VERSION_KEY, CURRENT_VERSION);
                        console.log('GeoJSON cached to localStorage');
                    } catch (e) {
                        console.warn('Failed to cache GeoJSON to localStorage:', e.message);
                    }
                    processGeoJson(data);
                } else {
                    throw new Error('No features in API response');
                }
            })
            .catch((err) => {
                console.error('GeoJSON load error:', err);
                if (self.map) {
                    L.marker([18.15, -66.5]).addTo(self.map).bindPopup('GeoJSON error: ' + err.message).openPopup();
                }
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
        const layer = e.target;
        layer.setStyle({
            fillColor: theme.highlightFill,
            weight: 2,
            color: theme.highlightBorder,
            fillOpacity: 0.6
        });
    },
    resetHighlight: function (e) {
        if (this.geojsonLayer) {
            this.geojsonLayer.resetStyle(e.target);
        }
    },
    highlightMunicipality: function (county) {
        if (!this.geojsonLayer)
            return;
        const theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        const self = this;
        this.geojsonLayer.eachLayer((layer) => {
            const geoLayer = layer;
            const feature = geoLayer.feature;
            if (feature && feature.properties) {
                const props = feature.properties;
                if (props.COUNTY === county) {
                    layer.setStyle({
                        fillColor: theme.highlightFill,
                        weight: 2,
                        color: theme.highlightBorder,
                        fillOpacity: 0.6
                    });
                }
                else {
                    self.geojsonLayer.resetStyle(layer);
                }
            }
        });
    },
    showSpeciesLocations: function (speciesName, locations) {
        this.clearSpeciesLocations();
        if (!locations || locations.length === 0)
            return;
        const self = this;
        locations.forEach(loc => {
            const circle = L.circle([loc.latitude, loc.longitude], {
                radius: loc.radiusMeters,
                fillColor: '#ef4444',
                color: '#dc2626',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map);
            const popupContent = loc.description
                ? `<strong>${speciesName}</strong><br/>${loc.description}`
                : `<strong>${speciesName}</strong>`;
            circle.bindPopup(popupContent);
            self.locationCircles.push(circle);
        });
        if (this.locationCircles.length > 0) {
            const group = L.featureGroup(this.locationCircles);
            this.map.fitBounds(group.getBounds(), { padding: [50, 50] });
        }
    },
    clearSpeciesLocations: function () {
        const self = this;
        this.locationCircles.forEach((circle) => {
            self.map.removeLayer(circle);
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
            this.locationCircles.forEach((circle) => circle.closePopup());
        }
    },
    showSearchRadius: function (radiusMeters) {
        if (!this.userLocation || !this.map)
            return;
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
        this.map.fitBounds(this.searchRadiusCircle.getBounds(), { padding: [50, 50] });
    },
    clearSearchRadius: function () {
        if (this.searchRadiusCircle && this.map) {
            this.map.removeLayer(this.searchRadiusCircle);
            this.searchRadiusCircle = null;
        }
        this.clearNearbySpeciesMarkers();
    },
    showNearbySpecies: function (species) {
        this.clearNearbySpeciesMarkers();
        const speciesArray = Array.isArray(species) ? species : [];
        if (speciesArray.length === 0 || !this.map)
            return;
        const self = this;
        speciesArray.forEach((s, index) => {
            const circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: '#10b981',
                color: '#059669',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map);
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
    clearNearbySpeciesMarkers: function () {
        const self = this;
        this.nearbySpeciesMarkers.forEach((marker) => {
            self.map.removeLayer(marker);
        });
        this.nearbySpeciesMarkers = [];
    },
    getUserLocation: function () {
        return this.userLocation;
    },
    focusOnNearbySpecies: function (index) {
        if (index >= 0 && index < this.nearbySpeciesMarkers.length) {
            const circle = this.nearbySpeciesMarkers[index];
            this.map.fitBounds(circle.getBounds(), { padding: [50, 50], maxZoom: 14 });
            circle.openPopup();
        }
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
    getSpeciesColor: function (speciesId, index) {
        if (this.speciesColorMap.has(speciesId)) {
            return this.speciesColorMap.get(speciesId);
        }
        const color = this.speciesColorPalette[index % this.speciesColorPalette.length];
        this.speciesColorMap.set(speciesId, color);
        return color;
    },
    showSpeciesLocationCircles: function (species) {
        this.clearSpeciesLocationCircles();
        const speciesArray = Array.isArray(species) ? species : [];
        if (speciesArray.length === 0 || !this.map)
            return;
        const self = this;
        const uniqueSpecies = new Map();
        speciesArray.forEach((s, idx) => {
            if (!uniqueSpecies.has(s.id)) {
                uniqueSpecies.set(s.id, uniqueSpecies.size);
            }
        });
        speciesArray.forEach((s) => {
            const colorIndex = uniqueSpecies.get(s.id) || 0;
            const color = self.getSpeciesColor(s.id, colorIndex);
            const circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: color,
                color: color,
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map);
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
    clearSpeciesLocationCircles: function () {
        const self = this;
        this.speciesLocationCircles.forEach((circle) => {
            self.map.removeLayer(circle);
        });
        this.speciesLocationCircles = [];
    },
    getSpeciesColors: function () {
        const result = [];
        this.speciesColorMap.forEach((color, id) => {
            result.push({ id, color });
        });
        return result;
    },
    resetSpeciesColors: function () {
        this.speciesColorMap.clear();
    }
};
function downloadFile(base64, fileName, contentType) {
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
window.downloadFile = downloadFile;
