// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

// Notificaciones Web Push (Sprint 8, ver SPEC-004 "Notificaciones push"): duplicado a propósito en
// service-worker.published.js (que no corre en `dotnet run`/dev) — el cacheo de assets es lo único
// que de verdad conviene omitir en dev, recibir/mostrar push no depende de eso. Mantener ambos en
// sincronía si se cambia este bloque.
self.addEventListener('push', event => {
    const data = event.data ? event.data.json() : {};
    event.waitUntil(self.registration.showNotification(data.title || 'TarjetasCredito', {
        body: data.body || '',
        icon: '/icon-192.png',
        data: { url: data.url || '/pagos' }
    }));
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data?.url || '/pagos'));
});
