/* هسته‌ی مشترک تعاملات نقشه و نمودار با Blazor. */
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

    /* ارقام لاتین را به ارقام فارسی تبدیل می‌کند. */

    var faDigits = (value) => String(value).replace(/\d/g, digit => "Û°Û±Û²Û³Û´ÛµÛ¶Û·Û¸Û¹"[digit]);
    /* متن را برای درج امن در HTML پاک‌سازی می‌کند. */
    var escapeHtml = (value) => String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");

    /* تغییرات ویرایشگر راه را به کامپوننت Blazor اطلاع می‌دهد. */

    var notifyRoadEditorChanged = () => {
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

    /* انتخاب نقاط مسیر را پاک‌سازی می‌کند. */

    var clearRoadRouteSelection = () => {
        if (roadRouteSelectionLayer) roadRouteSelectionLayer.remove();
        roadRouteSelectionLayer = null;
        roadRouteSelectionPoints = [];
        roadRouteSelectionActive = false;
        if (roadEditorMap)
            roadEditorMap.getContainer().classList.remove("route-point-selection");
    };

    /* یک نقطه را برای مسیر راه انتخاب می‌کند. */

    var selectRoadRoutePoint = (latlng) => {
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

    /* مسیر دریافتی را به نقاط مختصات معتبر تبدیل می‌کند. */

    var normalizeRoadPath = (path) => {
        if (!Array.isArray(path))
            throw new TypeError("Road path must be an array of points.");
        return path.map(point => ({
            latitude: Number(point.latitude),
            longitude: Number(point.longitude)
        })).filter(point => Number.isFinite(point.latitude) && Number.isFinite(point.longitude));
    };

    /* فاصله‌ی تقریبی نقطه تا قطعه‌ی مسیر را محاسبه می‌کند. */

    var distanceToSegmentSquared = (point, start, end) => {
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

    /* نزدیک‌ترین محل درج نقطه در مسیر را پیدا می‌کند. */

    var nearestRoadEditorInsertion = (point, path) => {
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

    /* نقشه‌ی موقعیت حادثه و لایه‌های آن را آزاد می‌کند. */

    var disposeAccidentLocationMap = () => {
        if (accidentLocationMap) accidentLocationMap.remove();
        accidentLocationMap = null;
        accidentLocationRoadLayer = null;
        accidentLocationMarker = null;
        accidentLocationDotNet = null;
    };

    /* تغییر مختصات حادثه را به Blazor اعلام می‌کند. */

    var notifyAccidentLocationChanged = (latitude, longitude, accuracy) => {
        if (!accidentLocationDotNet) return;
        accidentLocationDotNet.invokeMethodAsync(
            "OnAccidentLocationChanged",
            latitude,
            longitude,
            Number.isFinite(accuracy) ? accuracy : null
        ).catch(() => { });
    };

    /* نشانگر موقعیت حادثه را روی نقشه قرار می‌دهد. */

    var placeAccidentLocationMarker = (latitude, longitude, accuracy, notify) => {
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
            title: "Ù…ÙˆÙ‚Ø¹ÛŒØª Ø­Ø§Ø¯Ø«Ù‡"
        }).addTo(accidentLocationMap);
        accidentLocationMarker.on("dragend", event => {
            const point = event.target.getLatLng();
            notifyAccidentLocationChanged(point.lat, point.lng, null);
        });

        if (notify)
            notifyAccidentLocationChanged(latitude, longitude, accuracy);
    };

    /* تنظیمات مشترک نمودارهای Chart.js را اعمال می‌کند. */

    var chartDefaults = () => {
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

    /* یک نمودار را روی بوم مشخص‌شده ایجاد یا جایگزین می‌کند. */

    function createChart(id, config) {
        const canvas = document.getElementById(id);
        if (!canvas || !window.Chart) return;
        const existing = Chart.getChart(canvas);
        if (existing) existing.destroy();
        charts.push(new Chart(canvas, config));
    }

