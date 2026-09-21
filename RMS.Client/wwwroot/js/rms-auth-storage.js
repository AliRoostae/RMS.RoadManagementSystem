/* مدیریت نشست احراز هویت در حافظه‌ی مرورگر. */
window.rmsAuthStorage = {
    key: "rms.auth.session",
    /* نشست ذخیره‌شده‌ی کاربر را از حافظه می‌خواند. */
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
    /* نشست کاربر را در حافظه‌ی مناسب ذخیره می‌کند. */
    set: function (session, persistent) {
        this.clear();
        const storage = persistent ? window.localStorage : window.sessionStorage;
        storage.setItem(this.key, JSON.stringify(session));
    },
    /* نشست کاربر را از حافظه‌های مرورگر حذف می‌کند. */
    clear: function () {
        window.localStorage.removeItem(this.key);
        window.sessionStorage.removeItem(this.key);
    }
};

