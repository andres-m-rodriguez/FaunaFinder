/// <reference types="leaflet" />
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
    setApiBaseUrl: function (url) {
        this.apiBaseUrl = url.replace(/\/$/, '');
    },
    initMap: function (dotNetHelper, apiBaseUrl) {
        var _this = this;
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
        var savedDarkMode = localStorage.getItem('faunafinder-darkmode');
        if (savedDarkMode !== null) {
            this.isDarkMode = savedDarkMode === 'true';
        }
        else {
            this.isDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
        }
        var PR_BOUNDS = [
            [17.1176, -67.9426],
            [19.42, -64.9007]
        ];
        var zoom = this.isMobile ? 8 : 9;
        this.map = L.map('map', {
            center: [18.15, -66.5],
            zoom: zoom,
            maxBounds: PR_BOUNDS,
            maxBoundsViscosity: 1.0,
            scrollWheelZoom: !this.isMobile
        });
        var tileUrl = this.isDarkMode ? this.darkTileUrl : this.lightTileUrl;
        var attribution = this.isDarkMode
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
        window.addEventListener('resize', function () {
            _this.isMobile = window.innerWidth < 640;
        });
    },
    createLocateControl: function () {
        var self = this;
        var LocateControl = L.Control.extend({
            options: {
                position: 'topleft'
            },
            onAdd: function () {
                var container = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-locate');
                var button = L.DomUtil.create('a', 'leaflet-control-locate-button', container);
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
        var self = this;
        if (!navigator.geolocation) {
            self.showLocationError('geolocation_unsupported');
            return;
        }
        this.isLocating = true;
        this.setLocateControlState('loading');
        navigator.geolocation.getCurrentPosition(function (position) {
            var _a;
            self.isLocating = false;
            self.setLocateControlState('default');
            var lat = position.coords.latitude;
            var lng = position.coords.longitude;
            self.userLocation = { latitude: lat, longitude: lng };
            if (self.userLocationMarker) {
                self.map.removeLayer(self.userLocationMarker);
            }
            var userIcon = L.divIcon({
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
            (_a = self.dotNetHelper) === null || _a === void 0 ? void 0 : _a.invokeMethodAsync('OnUserLocationFound', lat, lng);
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
        var button = this.locateControl.querySelector('.leaflet-control-locate-button');
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
        var messages = {
            'geolocation_unsupported': 'Geolocation is not supported by your browser.',
            'permission_denied': 'Location access was denied. Please enable location permissions.',
            'position_unavailable': 'Unable to determine your location.',
            'timeout': 'Location request timed out. Please try again.',
            'unknown': 'An unknown error occurred while getting your location.'
        };
        var message = messages[errorType] || messages['unknown'];
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
            var tileUrl = isDark ? this.darkTileUrl : this.lightTileUrl;
            var attribution = isDark
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
        var self = this;
        var CACHE_KEY = 'pr-municipios-geojson';
        var VERSION_KEY = 'pr-municipios-version';
        var CURRENT_VERSION = 'v1';
        var processGeoJson = function (data) {
            var _a;
            self.geojsonLayer = L.geoJSON(data, {
                style: function () { return self.getDefaultStyle(); },
                onEachFeature: function (feature, layer) {
                    var props = feature.properties;
                    var name = props.NAME;
                    var state = props.STATE;
                    var county = props.COUNTY;
                    var geoJsonId = state + county;
                    layer.bindTooltip(name, {
                        direction: 'center',
                        className: 'municipality-tooltip'
                    });
                    layer.on({
                        mouseover: function (e) { return self.highlightFeature(e); },
                        mouseout: function (e) { return self.resetHighlight(e); },
                        click: function () { var _a; return (_a = self.dotNetHelper) === null || _a === void 0 ? void 0 : _a.invokeMethodAsync('OnMunicipalityClick', geoJsonId, name); }
                    });
                }
            }).addTo(self.map);
            console.log('GeoJSON loaded successfully with', data.features.length, 'features');
            // Notify Blazor that the map is fully loaded
            (_a = self.dotNetHelper) === null || _a === void 0 ? void 0 : _a.invokeMethodAsync('OnMapReady');
        };
        // Check localStorage cache first
        var cached = localStorage.getItem(CACHE_KEY);
        var cacheVersion = localStorage.getItem(VERSION_KEY);
        if (cached && cacheVersion === CURRENT_VERSION) {
            try {
                var data = JSON.parse(cached);
                if (data.features && data.features.length > 0) {
                    console.log('GeoJSON loaded from localStorage cache');
                    processGeoJson(data);
                    return;
                }
            }
            catch (e) {
                console.warn('Failed to parse cached GeoJSON, fetching from API');
            }
        }
        // Load from API endpoint
        var apiUrl = this.apiBaseUrl ? this.apiBaseUrl + '/api/municipalities/geojson' : '/api/municipalities/geojson';
        fetch(apiUrl)
            .then(function (r) {
            if (!r.ok)
                throw new Error('API returned ' + r.status);
            return r.json();
        })
            .then(function (data) {
            if (data.features && data.features.length > 0) {
                // Store in localStorage for future use
                try {
                    localStorage.setItem(CACHE_KEY, JSON.stringify(data));
                    localStorage.setItem(VERSION_KEY, CURRENT_VERSION);
                    console.log('GeoJSON cached to localStorage');
                }
                catch (e) {
                    console.warn('Failed to cache GeoJSON to localStorage:', e.message);
                }
                processGeoJson(data);
            }
            else {
                throw new Error('No features in API response');
            }
        })
            .catch(function (err) {
            console.error('GeoJSON load error:', err);
            if (self.map) {
                L.marker([18.15, -66.5]).addTo(self.map).bindPopup('GeoJSON error: ' + err.message).openPopup();
            }
        });
    },
    getDefaultStyle: function () {
        var theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
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
        var theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        var layer = e.target;
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
        var theme = this.isDarkMode ? this.darkTheme : this.lightTheme;
        var self = this;
        this.geojsonLayer.eachLayer(function (layer) {
            var geoLayer = layer;
            var feature = geoLayer.feature;
            if (feature && feature.properties) {
                var props = feature.properties;
                if (props.COUNTY === county) {
                    geoLayer.setStyle({
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
        var self = this;
        locations.forEach(function (loc) {
            var circle = L.circle([loc.latitude, loc.longitude], {
                radius: loc.radiusMeters,
                fillColor: '#ef4444',
                color: '#dc2626',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map);
            var popupContent = loc.description
                ? "<strong>".concat(speciesName, "</strong><br/>").concat(loc.description)
                : "<strong>".concat(speciesName, "</strong>");
            circle.bindPopup(popupContent);
            self.locationCircles.push(circle);
        });
        if (this.locationCircles.length > 0) {
            var group = L.featureGroup(this.locationCircles);
            this.map.fitBounds(group.getBounds(), { padding: [50, 50] });
        }
    },
    clearSpeciesLocations: function () {
        var self = this;
        this.locationCircles.forEach(function (circle) {
            self.map.removeLayer(circle);
        });
        this.locationCircles = [];
    },
    focusOnLocation: function (index) {
        if (index >= 0 && index < this.locationCircles.length) {
            var circle = this.locationCircles[index];
            this.map.fitBounds(circle.getBounds(), { padding: [50, 50], maxZoom: 14 });
            circle.openPopup();
        }
    },
    focusAllLocations: function () {
        if (this.locationCircles.length > 0) {
            var group = L.featureGroup(this.locationCircles);
            this.map.fitBounds(group.getBounds(), { padding: [50, 50] });
            this.locationCircles.forEach(function (circle) { return circle.closePopup(); });
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
        var speciesArray = Array.isArray(species) ? species : [];
        if (speciesArray.length === 0 || !this.map)
            return;
        var self = this;
        speciesArray.forEach(function (s) {
            var circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: '#10b981',
                color: '#059669',
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map);
            var distanceText = s.distanceMeters < 1000
                ? "".concat(Math.round(s.distanceMeters), "m away")
                : "".concat((s.distanceMeters / 1000).toFixed(1), "km away");
            var popupContent = "\n                <div class=\"nearby-species-popup\">\n                    <strong>".concat(s.commonName, "</strong><br/>\n                    <em>").concat(s.scientificName, "</em><br/>\n                    <span class=\"distance\">").concat(distanceText, "</span>\n                    ").concat(s.locationDescription ? "<br/><small>".concat(s.locationDescription, "</small>") : '', "\n                </div>\n            ");
            circle.bindPopup(popupContent);
            self.nearbySpeciesMarkers.push(circle);
        });
    },
    clearNearbySpeciesMarkers: function () {
        var self = this;
        this.nearbySpeciesMarkers.forEach(function (marker) {
            self.map.removeLayer(marker);
        });
        this.nearbySpeciesMarkers = [];
    },
    getUserLocation: function () {
        return this.userLocation;
    },
    focusOnNearbySpecies: function (index) {
        if (index >= 0 && index < this.nearbySpeciesMarkers.length) {
            var circle = this.nearbySpeciesMarkers[index];
            this.map.fitBounds(circle.getBounds(), { padding: [50, 50], maxZoom: 14 });
            circle.openPopup();
        }
    },
    getSpeciesColor: function (speciesId, index) {
        if (this.speciesColorMap.has(speciesId)) {
            return this.speciesColorMap.get(speciesId);
        }
        var color = this.speciesColorPalette[index % this.speciesColorPalette.length];
        this.speciesColorMap.set(speciesId, color);
        return color;
    },
    showSpeciesLocationCircles: function (species) {
        this.clearSpeciesLocationCircles();
        var speciesArray = Array.isArray(species) ? species : [];
        if (speciesArray.length === 0 || !this.map)
            return;
        var self = this;
        var uniqueSpecies = new Map();
        speciesArray.forEach(function (s) {
            if (!uniqueSpecies.has(s.id)) {
                uniqueSpecies.set(s.id, uniqueSpecies.size);
            }
        });
        speciesArray.forEach(function (s) {
            var colorIndex = uniqueSpecies.get(s.id) || 0;
            var color = self.getSpeciesColor(s.id, colorIndex);
            var circle = L.circle([s.latitude, s.longitude], {
                radius: s.radiusMeters,
                fillColor: color,
                color: color,
                weight: 2,
                fillOpacity: 0.35
            }).addTo(self.map);
            var distanceText = s.distanceMeters < 1000
                ? "".concat(Math.round(s.distanceMeters), "m away")
                : "".concat((s.distanceMeters / 1000).toFixed(1), "km away");
            var popupContent = "\n                <div class=\"nearby-species-popup\">\n                    <div style=\"display: flex; align-items: center; gap: 8px; margin-bottom: 4px;\">\n                        <span style=\"display: inline-block; width: 12px; height: 12px; border-radius: 50%; background-color: ".concat(color, ";\"></span>\n                        <strong>").concat(s.commonName, "</strong>\n                    </div>\n                    <em>").concat(s.scientificName, "</em><br/>\n                    <span class=\"distance\">").concat(distanceText, "</span>\n                    ").concat(s.locationDescription ? "<br/><small>".concat(s.locationDescription, "</small>") : '', "\n                </div>\n            ");
            circle.bindPopup(popupContent);
            self.speciesLocationCircles.push(circle);
        });
    },
    clearSpeciesLocationCircles: function () {
        var self = this;
        this.speciesLocationCircles.forEach(function (circle) {
            self.map.removeLayer(circle);
        });
        this.speciesLocationCircles = [];
    },
    getSpeciesColors: function () {
        var result = [];
        this.speciesColorMap.forEach(function (color, id) {
            result.push({ id: id, color: color });
        });
        return result;
    },
    resetSpeciesColors: function () {
        this.speciesColorMap.clear();
    },
    initSightingMap: function (containerId, latitude, longitude) {
        var container = document.getElementById(containerId);
        if (!container) {
            console.error('Map container not found:', containerId);
            return;
        }
        // Check if map already exists on this container
        if (container._leaflet_id) {
            return;
        }
        var savedDarkMode = localStorage.getItem('faunafinder-darkmode');
        var isDarkMode = savedDarkMode !== null
            ? savedDarkMode === 'true'
            : window.matchMedia('(prefers-color-scheme: dark)').matches;
        var tileUrl = isDarkMode
            ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
            : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
        var attribution = isDarkMode
            ? '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>'
            : '&copy; OpenStreetMap';
        var map = L.map(containerId, {
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
        var markerIcon = L.divIcon({
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
        setTimeout(function () {
            map.invalidateSize();
        }, 100);
    }
};
// ============================================================================
// Download File Function
// ============================================================================
/**
 * Download a file from base64 content
 */
function downloadFile(base64, fileName, contentType) {
    var byteCharacters = atob(base64);
    var byteNumbers = new Array(byteCharacters.length);
    for (var i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    var byteArray = new Uint8Array(byteNumbers);
    var blob = new Blob([byteArray], { type: contentType });
    var link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
}
window.downloadFile = downloadFile;
