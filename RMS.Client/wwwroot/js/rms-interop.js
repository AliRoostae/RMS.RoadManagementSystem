/* فایل اصلی تعاملات جاوااسکریپت؛ ماژول‌ها را به‌ترتیب وابستگی بارگذاری می‌کند. */
(function () {
    const modules = [
        "js/rms-interop-core.js",
        "js/rms-interop-charts.js",
        "js/rms-interop-roads.js",
        "js/rms-interop-accidents.js",
        "js/rms-auth-storage.js"
    ];

    // بارگذاری ترتیبی برای آماده‌بودن توابع قبل از اجرای Blazor.
    modules.forEach(source => {
        document.write(`<script src="${source}"><\/script>`);
    });
})();
