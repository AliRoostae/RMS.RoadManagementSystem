/* توابع نقشه و ویرایش هندسه‌ی راه‌ها. */
window.rmsInterop = window.rmsInterop || {};

Object.assign(window.rmsInterop, {
/* نقشه‌ی راه‌ها را نمایش می‌دهد. */
    renderRoadMap: function (elementId, roads) {
            if (!window.L) return;
            const element = document.getElementById(elementId);
            if (!element) return;
            if (roadMap) roadMap.remove();
            roadLayers = new Map();

            roadMap = L.map(elementId, { zoomControl: false, attributionControl: true }).setView([35.6892, 51.3890], 10);
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: "&copy; OpenStreetMap contributors"
            }).addTo(roadMap);
            L.control.zoom({ position: "bottomleft" }).addTo(roadMap);

            const bounds = [];
            (roads || []).forEach((road) => {
                const paths = (road.paths || [])
                    .map(path => (path || []).map(point => [point.latitude, point.longitude]))
                    .filter(path => path.length >= 2);
                if (!paths.length) return;
                const layer = L.polyline(paths, { color: "#4f46e5", weight: 4, opacity: .85 })
                    .addTo(roadMap)
                    .bindPopup(`<strong>${escapeHtml(road.name)}</strong><br><span dir="ltr">${escapeHtml(road.id)}</span>`);
                roadLayers.set(String(road.id).toLowerCase(), layer);
                paths.flat().forEach(point => bounds.push(point));
            });
            if (bounds.length) roadMap.fitBounds(bounds, { padding: [28, 28], maxZoom: 13 });
            setTimeout(() => roadMap && roadMap.invalidateSize(), 150);
        },

/* نقشه را روی راه انتخاب‌شده متمرکز می‌کند. */
    focusRoad: function (id) {
            if (!roadMap) return;
            const layer = roadLayers.get(String(id).toLowerCase());
            if (!layer) return;
            roadMap.fitBounds(layer.getBounds(), { padding: [35, 35], maxZoom: 14 });
            layer.openPopup();
        },

/* نقشه‌ی ویرایش هندسه‌ی راه را آماده می‌کند. */
    renderRoadEditorMap: function (elementId, initialPaths, dotNetReference) {
            if (!window.L) return;
            const element = document.getElementById(elementId);
            if (!element) return;
            clearRoadRouteSelection();
            if (roadEditorMap) roadEditorMap.remove();

            roadEditorLayer = null;
            roadEditorMarkerLayer = null;
            roadEditorSelectedPathIndex = -1;
            roadEditorSelectedIndex = -1;
            roadEditorDotNet = dotNetReference;

            roadEditorPaths = (initialPaths || [])
                .map(path => (path || []).map(point => ({
                    latitude: Number(point.latitude),
                    longitude: Number(point.longitude)
                })))
                .filter(path => path.length > 0);
            roadEditorSelectedPathIndex = roadEditorPaths.length ? 0 : -1;

            const initialPoint = roadEditorPaths.flat()[0];
            const initialCenter = initialPoint
                ? [initialPoint.latitude, initialPoint.longitude]
                : [35.6892, 51.3890];
            roadEditorMap = L.map(elementId, { zoomControl: true, doubleClickZoom: false }).setView(initialCenter, initialPoint ? 11 : 9);
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: "&copy; OpenStreetMap contributors"
            }).addTo(roadEditorMap);

            const redrawShape = () => {
                if (roadEditorLayer) roadEditorLayer.remove();
                roadEditorLayer = L.featureGroup().addTo(roadEditorMap);
                if (!roadEditorPaths.length) {
                    roadEditorLayer.remove();
                    roadEditorLayer = null;
                    return;
                }
                roadEditorPaths.forEach((path, pathIndex) => {
                    if (path.length < 2) return;
                    const active = pathIndex === roadEditorSelectedPathIndex;
                    const line = L.polyline(
                        path.map(point => [point.latitude, point.longitude]),
                        {
                            color: active ? "#4f46e5" : "#64748b",
                            weight: active ? 5 : 3,
                            opacity: active ? .95 : .62
                        })
                        .addTo(roadEditorLayer);
                    line.on("click", event => {
                        if (event.originalEvent) L.DomEvent.stopPropagation(event.originalEvent);
                        if (selectRoadRoutePoint(event.latlng)) return;
                        roadEditorSelectedPathIndex = pathIndex;
                        roadEditorSelectedIndex = -1;
                        redrawRoadEditor();
                        notifyRoadEditorChanged();
                    });
                });
            };

            const redrawMarkers = () => {
                if (roadEditorMarkerLayer) roadEditorMarkerLayer.remove();
                roadEditorMarkerLayer = L.layerGroup().addTo(roadEditorMap);
                const activePath = roadEditorPaths[roadEditorSelectedPathIndex] || [];
                activePath.forEach((point, index) => {
                    const selected = index === roadEditorSelectedIndex;
                    const icon = L.divIcon({
                        className: "",
                        html: `<span class="rms-road-vertex${selected ? " selected" : ""}" title="Ù†Ù‚Ø·Ù‡ ${faDigits(index + 1)}"></span>`,
                        iconSize: [18, 18],
                        iconAnchor: [9, 9]
                    });
                    const marker = L.marker([point.latitude, point.longitude], {
                        icon,
                        draggable: true,
                        keyboard: true,
                        title: `Ù†Ù‚Ø·Ù‡ ${faDigits(index + 1)}`
                    }).addTo(roadEditorMarkerLayer);

                    marker.on("click", event => {
                        if (event.originalEvent) L.DomEvent.stopPropagation(event.originalEvent);
                        if (selectRoadRoutePoint(event.target.getLatLng())) return;
                        roadEditorSelectedIndex = index;
                        redrawMarkers();
                        notifyRoadEditorChanged();
                    });
                    marker.on("drag", event => {
                        const location = event.target.getLatLng();
                        activePath[index] = { latitude: location.lat, longitude: location.lng };
                        redrawShape();
                    });
                    marker.on("dragend", event => {
                        const location = event.target.getLatLng();
                        activePath[index] = { latitude: location.lat, longitude: location.lng };
                        roadEditorSelectedIndex = index;
                        redrawShape();
                        redrawMarkers();
                        notifyRoadEditorChanged();
                    });
                });
            };

            redrawRoadEditor = () => {
                redrawShape();
                redrawMarkers();
            };

            roadEditorMap.on("click", event => {
                if (selectRoadRoutePoint(event.latlng)) return;
                const point = { latitude: event.latlng.lat, longitude: event.latlng.lng };
                if (roadEditorSelectedPathIndex < 0 || !roadEditorPaths[roadEditorSelectedPathIndex]) {
                    roadEditorPaths.push([]);
                    roadEditorSelectedPathIndex = roadEditorPaths.length - 1;
                }
                const activePath = roadEditorPaths[roadEditorSelectedPathIndex];
                const insertionIndex = nearestRoadEditorInsertion(point, activePath);
                activePath.splice(insertionIndex, 0, point);
                roadEditorSelectedIndex = insertionIndex;
                redrawRoadEditor();
                notifyRoadEditorChanged();
            });
            redrawRoadEditor();
            if (roadEditorPaths.some(path => path.length >= 2) && roadEditorLayer)
                roadEditorMap.fitBounds(roadEditorLayer.getBounds(), { padding: [24, 24], maxZoom: 14 });
            setTimeout(() => {
                if (roadEditorMap) roadEditorMap.invalidateSize();
                notifyRoadEditorChanged();
            }, 150);
        },

/* مسیرهای فعلی ویرایشگر راه را برمی‌گرداند. */
    getRoadEditorPaths: function () {
            return roadEditorPaths;
        },

/* حالت انتخاب ابتدا و انتهای مسیر را فعال می‌کند. */
    beginRoadRouteSelection: function () {
            if (!roadEditorMap) return;
            clearRoadRouteSelection();
            roadRouteSelectionActive = true;
            roadRouteSelectionLayer = L.layerGroup().addTo(roadEditorMap);
            roadEditorMap.getContainer().classList.add("route-point-selection");
        },

/* حالت انتخاب مسیر را لغو می‌کند. */
    cancelRoadRouteSelection: function () {
            clearRoadRouteSelection();
        },

/* مسیر اصلی ویرایشگر راه را تنظیم می‌کند. */
    setRoadEditorPrimaryPath: function (path) {
            const normalized = normalizeRoadPath(path);
            if (normalized.length < 2) return;
            if (roadEditorPaths.length)
                roadEditorPaths[0] = normalized;
            else
                roadEditorPaths.push(normalized);
            roadEditorSelectedPathIndex = 0;
            roadEditorSelectedIndex = -1;
            clearRoadRouteSelection();
            if (redrawRoadEditor) redrawRoadEditor();
            if (roadEditorLayer && roadEditorLayer.getBounds().isValid())
                roadEditorMap.fitBounds(roadEditorLayer.getBounds(), { padding: [24, 24], maxZoom: 15 });
            notifyRoadEditorChanged();
        },

/* یک مسیر جدید به ویرایشگر راه اضافه می‌کند. */
    appendRoadEditorPath: function (path) {
            const normalized = normalizeRoadPath(path);
            if (normalized.length < 2) return;
            roadEditorPaths.push(normalized);
            roadEditorSelectedPathIndex = roadEditorPaths.length - 1;
            roadEditorSelectedIndex = -1;
            clearRoadRouteSelection();
            if (redrawRoadEditor) redrawRoadEditor();
            if (roadEditorLayer && roadEditorLayer.getBounds().isValid())
                roadEditorMap.fitBounds(roadEditorLayer.getBounds(), { padding: [24, 24], maxZoom: 15 });
            notifyRoadEditorChanged();
        },

/* یک مسیر خالی جدید به ویرایشگر اضافه می‌کند. */
    addRoadEditorPath: function () {
            roadEditorPaths.push([]);
            roadEditorSelectedPathIndex = roadEditorPaths.length - 1;
            roadEditorSelectedIndex = -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

/* مسیر انتخاب‌شده را از ویرایشگر حذف می‌کند. */
    deleteSelectedRoadEditorPath: function () {
            if (roadEditorSelectedPathIndex < 0 || roadEditorSelectedPathIndex >= roadEditorPaths.length)
                return;
            roadEditorPaths.splice(roadEditorSelectedPathIndex, 1);
            roadEditorSelectedPathIndex = roadEditorPaths.length
                ? Math.min(roadEditorSelectedPathIndex, roadEditorPaths.length - 1)
                : -1;
            roadEditorSelectedIndex = -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

/* همه‌ی مسیرهای ترسیم‌شده را پاک می‌کند. */
    clearRoadEditorDrawing: function () {
            roadEditorPaths = [];
            roadEditorSelectedPathIndex = -1;
            roadEditorSelectedIndex = -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

/* نقطه‌ی انتخاب‌شده‌ی مسیر را حذف می‌کند. */
    deleteSelectedRoadEditorPoint: function () {
            const activePath = roadEditorPaths[roadEditorSelectedPathIndex];
            if (!activePath || roadEditorSelectedIndex < 0 || roadEditorSelectedIndex >= activePath.length)
                return;
            activePath.splice(roadEditorSelectedIndex, 1);
            roadEditorSelectedIndex = activePath.length
                ? Math.min(roadEditorSelectedIndex, activePath.length - 1)
                : -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

/* آخرین نقطه‌ی مسیر فعال را حذف می‌کند. */
    removeLastRoadEditorPoint: function () {
            const activePath = roadEditorPaths[roadEditorSelectedPathIndex];
            if (activePath && activePath.length) activePath.pop();
            roadEditorSelectedIndex = activePath && activePath.length ? activePath.length - 1 : -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

/* منابع نقشه‌ی ویرایشگر راه را آزاد می‌کند. */
    disposeRoadEditorMap: function () {
            clearRoadRouteSelection();
            if (roadEditorMap) roadEditorMap.remove();
            roadEditorMap = null;
            roadEditorLayer = null;
            roadEditorMarkerLayer = null;
            roadEditorPaths = [];
            roadEditorSelectedPathIndex = -1;
            roadEditorSelectedIndex = -1;
            roadEditorDotNet = null;
            redrawRoadEditor = null;
        }
});

