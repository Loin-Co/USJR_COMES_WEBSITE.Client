// offline-sync.js — localStorage queue + online/offline detection for WASM offline mode

window.offlineSync = {
    _dotnetRef: null,

    /** Returns true if the browser reports it is online */
    isOnline: () => navigator.onLine,

    /** Register a .NET object reference to receive online/offline callbacks */
    registerOnlineHandler: function (dotnetRef) {
        window.offlineSync._dotnetRef = dotnetRef;

        window.addEventListener('online', () => {
            if (window.offlineSync._dotnetRef) {
                window.offlineSync._dotnetRef.invokeMethodAsync('OnBrowserOnline');
            }
        });

        window.addEventListener('offline', () => {
            if (window.offlineSync._dotnetRef) {
                window.offlineSync._dotnetRef.invokeMethodAsync('OnBrowserOffline');
            }
        });
    },

    /** Dispose the .NET reference */
    dispose: function () {
        window.offlineSync._dotnetRef = null;
    },

    /** Read the pending operations queue from localStorage */
    getPendingOps: function () {
        try {
            return JSON.parse(localStorage.getItem('comes_pending_ops') || '[]');
        } catch { return []; }
    },

    /** Write the pending operations queue to localStorage */
    setPendingOps: function (ops) {
        try {
            localStorage.setItem('comes_pending_ops', JSON.stringify(ops));
        } catch { }
    },

    /** Add a single operation to the queue */
    enqueue: function (op) {
        const ops = window.offlineSync.getPendingOps();
        ops.push(op);
        window.offlineSync.setPendingOps(ops);
    },

    /** Remove an operation from the queue by its id */
    dequeue: function (id) {
        const ops = window.offlineSync.getPendingOps().filter(o => o.id !== id);
        window.offlineSync.setPendingOps(ops);
    },

    /** Clear all pending operations */
    clearAll: function () {
        localStorage.removeItem('comes_pending_ops');
    },

    /** Count of pending operations */
    pendingCount: function () {
        return window.offlineSync.getPendingOps().length;
    },

    // ── User session persistence ──────────────────────────────────────────────

    saveUserSession: function (sessionJson) {
        try { localStorage.setItem('comes_user_session', sessionJson); } catch { }
    },

    loadUserSession: function () {
        try { return localStorage.getItem('comes_user_session') || ''; } catch { return ''; }
    },

    clearUserSession: function () {
        try { localStorage.removeItem('comes_user_session'); } catch { }
    },

    // ── Last-sync timestamp ───────────────────────────────────────────────────

    saveLastSync: function (isoString) {
        try { localStorage.setItem('comes_last_sync', isoString); } catch { }
    },

    loadLastSync: function () {
        try { return localStorage.getItem('comes_last_sync') || ''; } catch { return ''; }
    },

    // ── Cookie consent ────────────────────────────────────────────────────────

    getCookieConsent: function () {
        try { return localStorage.getItem('comes_cookie_consent') || ''; } catch { return ''; }
    },

    setCookieConsent: function (choice) {
        try { localStorage.setItem('comes_cookie_consent', choice); } catch { }
    }
};
