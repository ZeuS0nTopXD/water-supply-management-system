(function () {
    const isSupported = 'serviceWorker' in navigator;

    async function registerPwa() {
        if (!isSupported) return;
        try {
            await navigator.serviceWorker.register('/service-worker.js', { scope: '/' });
        } catch (error) {
            console.warn('PWA registration failed', error);
        }
    }

    async function requestBillReminderPermission() {
        if (!('Notification' in window)) return 'unsupported';
        const permission = await Notification.requestPermission();
        if (permission === 'granted') new Notification('Water Supply reminders enabled', { body: 'You can receive bill reminders on this device.' });
        return permission;
    }

    window.WaterSupplyPwa = { registerPwa, requestBillReminderPermission };
    registerPwa();
})();
