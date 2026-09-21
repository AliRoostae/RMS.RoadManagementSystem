(function () {
    const charts = [];
    let roadMap;
    let roadLayers = new Map();
    let roadEditorMap;
    let roadEditorLayer;
    let roadEditorMarkerLayer;
    let roadEditorPaths = [];
    let roadEditorSelectedPathIndex = -1;
    let roadEditorSelectedIndex = -1;
    let roadEditorDotNet;
    let redrawRoadEditor;
    let roadRouteSelectionActive = false;
    let roadRouteSelectionLayer;
    let roadRouteSelectionPoints = [];
    let accidentLocationMap;
    let accidentLocationRoadLayer;
    let accidentLocationMarker;
    let accidentLocationDotNet;

    const faDigits = (value) => String(value).replace(/\d/g, digit => "۰۱۲۳۴۵۶۷۸۹"[digit]);
    const escapeHtml = (value) => String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");

    const notifyRoadEditorChanged = () => {
        if (!roadEditorDotNet) return;
        const pointCount = roadEditorPaths.reduce((total, path) => total + path.length, 0);
        roadEditorDotNet.invokeMethodAsync(
            "OnRoadEditorChanged",
            pointCount,
            roadEditorSelectedIndex >= 0 ? roadEditorSelectedIndex : null,
            roadEditorPaths.length,
            roadEditorSelectedPathIndex >= 0 ? roadEditorSelectedPathIndex : null
        ).catch(() => { });
    };

    const clearRoadRouteSelection = () => {
        if (roadRouteSelectionLayer) roadRouteSelectionLayer.remove();
        roadRouteSelectionLayer = null;
        roadRouteSelectionPoints = [];
        roadRouteSelectionActive = false;
        if (roadEditorMap)
            roadEditorMap.getContainer().classList.remove("route-point-selection");
    };

    const selectRoadRoutePoint = (latlng) => {
        if (!roadRouteSelectionActive || !roadEditorMap) return false;

        const point = { latitude: latlng.lat, longitude: latlng.lng };
        if (!roadRouteSelectionLayer)
            roadRouteSelectionLayer = L.layerGroup().addTo(roadEditorMap);
        roadRouteSelectionPoints.push(point);
        const pointNumber = roadRouteSelectionPoints.length;
        const icon = L.divIcon({
            className: "",
            html: `<span class="rms-route-endpoint ${pointNumber === 1 ? "start" : "end"}">${faDigits(pointNumber)}</span>`,
            iconSize: [28, 28],
            iconAnchor: [14, 14]
        });
        L.marker([point.latitude, point.longitude], { icon, interactive: false })
            .addTo(roadRouteSelectionLayer);

        if (pointNumber >= 2) {
            roadRouteSelectionActive = false;
            roadEditorMap.getContainer().classList.remove("route-point-selection");
        }
        if (roadEditorDotNet) {
            roadEditorDotNet.invokeMethodAsync(
                "OnRoadRoutePointSelected",
                point.latitude,
                point.longitude,
                pointNumber
            ).catch(() => { });
        }
        return true;
    };

    const normalizeRoadPath = (path) => {
        if (!Array.isArray(path))
            throw new TypeError("Road path must be an array of points.");
        return path.map(point => ({
            latitude: Number(point.latitude),
            longitude: Number(point.longitude)
        })).filter(point => Number.isFinite(point.latitude) && Number.isFinite(point.longitude));
    };

    const distanceToSegmentSquared = (point, start, end) => {
        const latitudeScale = Math.cos(point.latitude * Math.PI / 180);
        const px = point.longitude * latitudeScale;
        const py = point.latitude;
        const sx = start.longitude * latitudeScale;
        const sy = start.latitude;
        const ex = end.longitude * latitudeScale;
        const ey = end.latitude;
        const dx = ex - sx;
        const dy = ey - sy;
        if (dx === 0 && dy === 0) return (px - sx) ** 2 + (py - sy) ** 2;
        const t = Math.max(0, Math.min(1, ((px - sx) * dx + (py - sy) * dy) / (dx * dx + dy * dy)));
        const nearestX = sx + t * dx;
        const nearestY = sy + t * dy;
        return (px - nearestX) ** 2 + (py - nearestY) ** 2;
    };

    const nearestRoadEditorInsertion = (point, path) => {
        if (path.length < 2) return path.length;
        let bestIndex = 1;
        let bestDistance = Number.POSITIVE_INFINITY;
        for (let index = 0; index < path.length - 1; index++) {
            const distance = distanceToSegmentSquared(
                point,
                path[index],
                path[index + 1]
            );
            if (distance < bestDistance) {
                bestDistance = distance;
                bestIndex = index + 1;
            }
        }
        const firstDistance = distanceToSegmentSquared(point, path[0], path[0]);
        const lastPoint = path[path.length - 1];
        const lastDistance = distanceToSegmentSquared(point, lastPoint, lastPoint);
        if (firstDistance <= bestDistance) return 0;
        if (lastDistance <= bestDistance) return path.length;
        return bestIndex;
    };

    const disposeAccidentLocationMap = () => {
        if (accidentLocationMap) accidentLocationMap.remove();
        accidentLocationMap = null;
        accidentLocationRoadLayer = null;
        accidentLocationMarker = null;
        accidentLocationDotNet = null;
    };

    const notifyAccidentLocationChanged = (latitude, longitude, accuracy) => {
        if (!accidentLocationDotNet) return;
        accidentLocationDotNet.invokeMethodAsync(
            "OnAccidentLocationChanged",
            latitude,
            longitude,
            Number.isFinite(accuracy) ? accuracy : null
        ).catch(() => { });
    };

    const placeAccidentLocationMarker = (latitude, longitude, accuracy, notify) => {
        if (!accidentLocationMap || !Number.isFinite(latitude) || !Number.isFinite(longitude)) return;
        if (accidentLocationMarker) accidentLocationMarker.remove();

        const icon = L.divIcon({
            className: "",
            html: '<span class="rms-accident-location-marker"><i class="bi bi-exclamation-lg"></i></span>',
            iconSize: [34, 42],
            iconAnchor: [17, 39]
        });
        accidentLocationMarker = L.marker([latitude, longitude], {
            icon,
            draggable: true,
            keyboard: true,
            title: "موقعیت حادثه"
        }).addTo(accidentLocationMap);
        accidentLocationMarker.on("dragend", event => {
            const point = event.target.getLatLng();
            notifyAccidentLocationChanged(point.lat, point.lng, null);
        });

        if (notify)
            notifyAccidentLocationChanged(latitude, longitude, accuracy);
    };

    const chartDefaults = () => {
        if (!window.Chart) return;
        Chart.defaults.font.family = "Vazirmatn, Tahoma, sans-serif";
        Chart.defaults.color = "#8490a2";
        Chart.defaults.locale = "fa-IR";
        Chart.defaults.animation.duration = 650;
    };

    const tooltipOptions = {
        rtl: true,
        titleFont: { family: "Vazirmatn", size: 11 },
        bodyFont: { family: "Vazirmatn", size: 10 },
        padding: 10,
        backgroundColor: "rgba(23, 32, 51, .94)",
        displayColors: true,
        cornerRadius: 8
    };

    function createChart(id, config) {
        const canvas = document.getElementById(id);
        if (!canvas || !window.Chart) return;
        const existing = Chart.getChart(canvas);
        if (existing) existing.destroy();
        charts.push(new Chart(canvas, config));
    }

    window.rmsInterop = {
        renderDashboardCharts: function (data) {
            if (!window.Chart) return;
            chartDefaults();
            data = data || {};

            const trendCanvas = document.getElementById("accidentTrendChart");
            const trendContext = trendCanvas && trendCanvas.getContext("2d");
            const gradient = trendContext && trendContext.createLinearGradient(0, 0, 0, 245);
            if (gradient) {
                gradient.addColorStop(0, "rgba(79, 70, 229, .25)");
                gradient.addColorStop(1, "rgba(79, 70, 229, 0)");
            }

            createChart("accidentTrendChart", {
                type: "line",
                data: {
                    labels: data.trendLabels || [],
                    datasets: [{
                        label: "تعداد حادثه",
                        data: data.trendValues || [],
                        borderColor: "#4f46e5",
                        backgroundColor: gradient || "rgba(79,70,229,.12)",
                        pointBackgroundColor: "#fff",
                        pointBorderColor: "#4f46e5",
                        pointBorderWidth: 2,
                        pointRadius: 3,
                        pointHoverRadius: 5,
                        borderWidth: 2.5,
                        tension: .38,
                        fill: true
                    }]
                },
                options: {
                    maintainAspectRatio: false,
                    interaction: { mode: "index", intersect: false },
                    plugins: { legend: { display: false }, tooltip: tooltipOptions },
                    scales: {
                        x: { grid: { display: false }, border: { display: false }, ticks: { font: { size: 9 } } },
                        y: { beginAtZero: true, border: { display: false }, grid: { color: "#eef1f5" }, ticks: { font: { size: 9 }, callback: faDigits } }
                    }
                }
            });

            createChart("causeChart", {
                type: "doughnut",
                data: {
                    labels: data.causeLabels || [],
                    datasets: [{ data: data.causeValues || [], backgroundColor: ["#4f46e5", "#f59e0b", "#06b6d4", "#ef4444", "#10b981", "#94a3b8"], borderWidth: 0, hoverOffset: 5 }]
                },
                options: { maintainAspectRatio: false, cutout: "72%", plugins: { legend: { display: false }, tooltip: tooltipOptions } }
            });
        },

        renderReportCharts: function (data) {
            if (!window.Chart) return;
            chartDefaults();
            data = data || {};

            createChart("reportComboChart", {
                data: {
                    labels: data.monthLabels || [],
                    datasets: [
                        { type: "bar", label: "تعداد حوادث", data: data.accidentCounts || [], backgroundColor: "rgba(79,70,229,.75)", borderRadius: 5, yAxisID: "y" },
                        { type: "line", label: "میانگین شدت", data: data.averageInjury || [], borderColor: "#f59e0b", backgroundColor: "#f59e0b", tension: .35, pointRadius: 3, yAxisID: "y1" }
                    ]
                },
                options: {
                    maintainAspectRatio: false,
                    plugins: { legend: { position: "bottom", rtl: true, labels: { usePointStyle: true, boxWidth: 7, padding: 18, font: { size: 9 } } }, tooltip: tooltipOptions },
                    scales: {
                        x: { grid: { display: false }, border: { display: false }, ticks: { font: { size: 9 } } },
                        y: { beginAtZero: true, border: { display: false }, grid: { color: "#eef1f5" }, ticks: { callback: faDigits } },
                        y1: { beginAtZero: true, position: "right", max: 100, border: { display: false }, grid: { display: false }, ticks: { callback: value => faDigits(value) + "٪" } }
                    }
                }
            });

            createChart("reportTypeChart", {
                type: "doughnut",
                data: { labels: data.typeLabels || [], datasets: [{ data: data.typeValues || [], backgroundColor: ["#4f46e5", "#06b6d4", "#f59e0b", "#ef4444", "#10b981", "#cbd5e1"], borderWidth: 0 }] },
                options: { maintainAspectRatio: false, cutout: "68%", plugins: { legend: { position: "bottom", rtl: true, labels: { usePointStyle: true, boxWidth: 7, font: { size: 8 } } }, tooltip: tooltipOptions } }
            });

            createChart("weatherChart", {
                type: "bar",
                data: { labels: data.weatherLabels || [], datasets: [{ label: "تعداد حادثه", data: data.weatherValues || [], backgroundColor: ["#fbbf24", "#94a3b8", "#38bdf8", "#c7d2fe", "#a5b4fc", "#fca5a5", "#86efac"], borderRadius: 6 }] },
                options: { maintainAspectRatio: false, plugins: { legend: { display: false }, tooltip: tooltipOptions }, scales: { x: { grid: { display: false }, border: { display: false } }, y: { beginAtZero: true, border: { display: false }, grid: { color: "#eef1f5" }, ticks: { callback: faDigits } } } }
            });
        },

        destroyCharts: function () {
            while (charts.length) {
                const chart = charts.pop();
                if (chart) chart.destroy();
            }
        },

        renderRoadMap: function (elementId, roads, accidents) {
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

            (accidents || []).forEach((accident) => {
                const icon = L.divIcon({ className: "", html: '<div class="rms-map-marker"></div>', iconSize: [22, 22], iconAnchor: [11, 20] });
                const point = [accident.latitude, accident.longitude];
                L.marker(point, { icon }).addTo(roadMap)
                    .bindPopup(`<strong>${escapeHtml(accident.title)}</strong><br>${escapeHtml(accident.date)}`);
                bounds.push(point);
            });

            if (bounds.length) roadMap.fitBounds(bounds, { padding: [28, 28], maxZoom: 13 });
            setTimeout(() => roadMap && roadMap.invalidateSize(), 150);
        },

        focusRoad: function (id) {
            if (!roadMap) return;
            const layer = roadLayers.get(String(id).toLowerCase());
            if (!layer) return;
            roadMap.fitBounds(layer.getBounds(), { padding: [35, 35], maxZoom: 14 });
            layer.openPopup();
        },

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
                        html: `<span class="rms-road-vertex${selected ? " selected" : ""}" title="نقطه ${faDigits(index + 1)}"></span>`,
                        iconSize: [18, 18],
                        iconAnchor: [9, 9]
                    });
                    const marker = L.marker([point.latitude, point.longitude], {
                        icon,
                        draggable: true,
                        keyboard: true,
                        title: `نقطه ${faDigits(index + 1)}`
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

        getRoadEditorPaths: function () {
            return roadEditorPaths;
        },

        beginRoadRouteSelection: function () {
            if (!roadEditorMap) return;
            clearRoadRouteSelection();
            roadRouteSelectionActive = true;
            roadRouteSelectionLayer = L.layerGroup().addTo(roadEditorMap);
            roadEditorMap.getContainer().classList.add("route-point-selection");
        },

        cancelRoadRouteSelection: function () {
            clearRoadRouteSelection();
        },

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

        addRoadEditorPath: function () {
            roadEditorPaths.push([]);
            roadEditorSelectedPathIndex = roadEditorPaths.length - 1;
            roadEditorSelectedIndex = -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

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

        clearRoadEditorDrawing: function () {
            roadEditorPaths = [];
            roadEditorSelectedPathIndex = -1;
            roadEditorSelectedIndex = -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

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

        removeLastRoadEditorPoint: function () {
            const activePath = roadEditorPaths[roadEditorSelectedPathIndex];
            if (activePath && activePath.length) activePath.pop();
            roadEditorSelectedIndex = activePath && activePath.length ? activePath.length - 1 : -1;
            if (redrawRoadEditor) redrawRoadEditor();
            notifyRoadEditorChanged();
        },

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
        },

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

        clearAccidentLocation: function () {
            if (accidentLocationMarker) accidentLocationMarker.remove();
            accidentLocationMarker = null;
        },

        setAccidentLocation: function (latitude, longitude) {
            placeAccidentLocationMarker(Number(latitude), Number(longitude), null, false);
        },

        disposeAccidentLocationMap: function () {
            disposeAccidentLocationMap();
        },

        disposeRoadMaps: function () {
            if (roadMap) roadMap.remove();
            roadMap = null;
            roadLayers = new Map();
            this.disposeRoadEditorMap();
            disposeAccidentLocationMap();
        }
    };
})();

window.rmsAuthStorage = {
    key: "rms.auth.session",
    get: function () {
        const value = window.localStorage.getItem(this.key) || window.sessionStorage.getItem(this.key);
        if (!value) return null;
        try {
            return JSON.parse(value);
        } catch {
            this.clear();
            return null;
        }
    },
    set: function (session, persistent) {
        this.clear();
        const storage = persistent ? window.localStorage : window.sessionStorage;
        storage.setItem(this.key, JSON.stringify(session));
    },
    clear: function () {
        window.localStorage.removeItem(this.key);
        window.sessionStorage.removeItem(this.key);
    }
};
