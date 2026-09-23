window.rmsInterop = window.rmsInterop || {};
let accidentReportMap;
Object.assign(window.rmsInterop, {
    renderAccidentReportMap: function (elementId, points) {
        if (!window.L) return;
        const element = document.getElementById(elementId);
        if (!element) return;
        if (accidentReportMap) accidentReportMap.remove();
        accidentReportMap = L.map(elementId, { zoomControl: true, attributionControl: true }).setView([35.6892, 51.3890], 6);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { maxZoom: 19, attribution: "&copy; OpenStreetMap contributors" }).addTo(accidentReportMap);
        const bounds = [];
        (points || []).forEach(point => {
            const latitude = Number(point.latitude), longitude = Number(point.longitude);
            if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) return;
            const icon = L.divIcon({ className: "", html: '<div class="rms-map-marker"></div>', iconSize: [22, 22], iconAnchor: [11, 20] });
            const popup = "<strong>" + escapeHtml(point.title) + "</strong><br>" + escapeHtml(point.date);
            L.marker([latitude, longitude], { icon }).addTo(accidentReportMap).bindPopup(popup);
            bounds.push([latitude, longitude]);
        });
        if (bounds.length) accidentReportMap.fitBounds(bounds, { padding: [28, 28], maxZoom: 14 });
        setTimeout(() => accidentReportMap && accidentReportMap.invalidateSize(), 150);
    },
    disposeAccidentReportMap: function () { if (accidentReportMap) accidentReportMap.remove(); accidentReportMap = null; }
});
