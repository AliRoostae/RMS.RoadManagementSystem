/* توابع نقشه و موقعیت حوادث. */
window.rmsInterop = window.rmsInterop || {};

Object.assign(window.rmsInterop, {
/* نقشه‌ی ثبت موقعیت حادثه را نمایش می‌دهد. */
    renderAccidentLocationMap: function (elementId, roadPaths, selectedPoint, dotNetReference) {
            if (!window.L) return false;
            const element = document.getElementById(elementId);
            if (!element) return false;

            disposeAccidentLocationMap();
            accidentLocationDotNet = dotNetReference;
            accidentLocationMap = L.map(elementId, {
                zoomControl: true,
                doubleClickZoom: true
            }).setView([32.4279, 53.6880], 5);
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: "&copy; OpenStreetMap contributors"
            }).addTo(accidentLocationMap);

            const paths = (Array.isArray(roadPaths) ? roadPaths : [])
                .map(path => normalizeRoadPath(path))
                .filter(path => path.length >= 2);
            if (paths.length) {
                accidentLocationRoadLayer = L.polyline(
                    paths.map(path => path.map(point => [point.latitude, point.longitude])),
                    { color: "#4f46e5", weight: 6, opacity: .86, lineCap: "round", lineJoin: "round" }
                ).addTo(accidentLocationMap);
                accidentLocationMap.fitBounds(accidentLocationRoadLayer.getBounds(), {
                    padding: [30, 30],
                    maxZoom: 16
                });
            }

            accidentLocationMap.on("click", event => {
                placeAccidentLocationMarker(event.latlng.lat, event.latlng.lng, null, true);
            });

            if (selectedPoint) {
                const latitude = Number(selectedPoint.latitude);
                const longitude = Number(selectedPoint.longitude);
                if (Number.isFinite(latitude) && Number.isFinite(longitude)) {
                    placeAccidentLocationMarker(latitude, longitude, null, false);
                    if (!paths.length)
                        accidentLocationMap.setView([latitude, longitude], 16);
                }
            }

            setTimeout(() => accidentLocationMap && accidentLocationMap.invalidateSize(), 150);
            return true;
        },

/* موقعیت فعلی دستگاه را برای حادثه دریافت می‌کند. */
    useCurrentAccidentLocation: function () {
            if (!accidentLocationDotNet) return;
            if (!navigator.geolocation) {
                accidentLocationDotNet.invokeMethodAsync("OnAccidentGeolocationError", 0).catch(() => { });
                return;
            }

            navigator.geolocation.getCurrentPosition(
                position => {
                    const latitude = position.coords.latitude;
                    const longitude = position.coords.longitude;
                    placeAccidentLocationMarker(latitude, longitude, position.coords.accuracy, true);
                    if (accidentLocationMap)
                        accidentLocationMap.setView([latitude, longitude], 17);
                },
                error => {
                    if (accidentLocationDotNet)
                        accidentLocationDotNet.invokeMethodAsync("OnAccidentGeolocationError", error?.code || 0).catch(() => { });
                },
                { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
            );
        },

/* نشانگر موقعیت حادثه را پاک می‌کند. */
    clearAccidentLocation: function () {
            if (accidentLocationMarker) accidentLocationMarker.remove();
            accidentLocationMarker = null;
        },

/* مختصات انتخاب‌شده‌ی حادثه را روی نقشه اعمال می‌کند. */
    setAccidentLocation: function (latitude, longitude) {
            placeAccidentLocationMarker(Number(latitude), Number(longitude), null, false);
        },

/* نقشه‌ی موقعیت حادثه و لایه‌های آن را آزاد می‌کند. */
    disposeAccidentLocationMap: function () {
            disposeAccidentLocationMap();
        },

/* همه‌ی نقشه‌های راه و حادثه را آزاد می‌کند. */
    disposeRoadMaps: function () {
            if (roadMap) roadMap.remove();
            roadMap = null;
            roadLayers = new Map();
            this.disposeRoadEditorMap();
            disposeAccidentLocationMap();
        }
});

