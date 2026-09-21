/* توابع نمودارهای داشبورد و گزارشات. */
window.rmsInterop = window.rmsInterop || {};

Object.assign(window.rmsInterop, {
/* نمودارهای داشبورد اصلی را رسم می‌کند. */
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
                        label: "ØªØ¹Ø¯Ø§Ø¯ Ø­Ø§Ø¯Ø«Ù‡",
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

/* نمودارهای صفحه‌ی گزارشات را رسم می‌کند. */
    renderReportCharts: function (data) {
            if (!window.Chart) return;
            chartDefaults();
            data = data || {};

            createChart("reportComboChart", {
                data: {
                    labels: data.monthLabels || [],
                    datasets: [
                        { type: "bar", label: "ØªØ¹Ø¯Ø§Ø¯ Ø­ÙˆØ§Ø¯Ø«", data: data.accidentCounts || [], backgroundColor: "rgba(79,70,229,.75)", borderRadius: 5, yAxisID: "y" },
                        { type: "line", label: "Ù…ÛŒØ§Ù†Ú¯ÛŒÙ† Ø´Ø¯Øª", data: data.averageInjury || [], borderColor: "#f59e0b", backgroundColor: "#f59e0b", tension: .35, pointRadius: 3, yAxisID: "y1" }
                    ]
                },
                options: {
                    maintainAspectRatio: false,
                    plugins: { legend: { position: "bottom", rtl: true, labels: { usePointStyle: true, boxWidth: 7, padding: 18, font: { size: 9 } } }, tooltip: tooltipOptions },
                    scales: {
                        x: { grid: { display: false }, border: { display: false }, ticks: { font: { size: 9 } } },
                        y: { beginAtZero: true, border: { display: false }, grid: { color: "#eef1f5" }, ticks: { callback: faDigits } },
                        y1: { beginAtZero: true, position: "right", max: 100, border: { display: false }, grid: { display: false }, ticks: { callback: value => faDigits(value) + "Ùª" } }
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
                data: { labels: data.weatherLabels || [], datasets: [{ label: "ØªØ¹Ø¯Ø§Ø¯ Ø­Ø§Ø¯Ø«Ù‡", data: data.weatherValues || [], backgroundColor: ["#fbbf24", "#94a3b8", "#38bdf8", "#c7d2fe", "#a5b4fc", "#fca5a5", "#86efac"], borderRadius: 6 }] },
                options: { maintainAspectRatio: false, plugins: { legend: { display: false }, tooltip: tooltipOptions }, scales: { x: { grid: { display: false }, border: { display: false } }, y: { beginAtZero: true, border: { display: false }, grid: { color: "#eef1f5" }, ticks: { callback: faDigits } } } }
            });
        },

/* همه‌ی نمودارهای فعال را آزاد می‌کند. */
    destroyCharts: function () {
            while (charts.length) {
                const chart = charts.pop();
                if (chart) chart.destroy();
            }
        }
});

