// Notificaciones Web Push (VAPID) — ver docs/specs/SPEC_TarjetasCredito-004-Seguridad.md
// "Notificaciones push". El usuario activa esto explícitamente desde Configuración (opt-in), nunca
// se pide el permiso del navegador de forma automática al iniciar sesión.
window.webPush = (function () {
    function disponible() {
        return !!(window.PushManager && 'serviceWorker' in navigator);
    }

    // El navegador espera la clave VAPID pública como Uint8Array (formato "applicationServerKey"),
    // no como el string base64url que devuelve el servidor.
    function base64UrlAUint8Array(base64Url) {
        const padding = '='.repeat((4 - (base64Url.length % 4)) % 4);
        const base64 = (base64Url + padding).replace(/-/g, '+').replace(/_/g, '/');
        const raw = window.atob(base64);
        const bytes = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; i++) {
            bytes[i] = raw.charCodeAt(i);
        }
        return bytes;
    }

    async function obtenerSuscripcionActual() {
        if (!disponible()) {
            return null;
        }
        const registro = await navigator.serviceWorker.ready;
        const suscripcion = await registro.pushManager.getSubscription();
        return suscripcion ? JSON.stringify(suscripcion.toJSON()) : null;
    }

    async function suscribir(vapidPublicKey) {
        const registro = await navigator.serviceWorker.ready;
        const suscripcion = await registro.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: base64UrlAUint8Array(vapidPublicKey)
        });
        return JSON.stringify(suscripcion.toJSON());
    }

    async function desuscribir() {
        const registro = await navigator.serviceWorker.ready;
        const suscripcion = await registro.pushManager.getSubscription();
        if (suscripcion) {
            await suscripcion.unsubscribe();
        }
    }

    return {
        disponible: disponible,
        obtenerSuscripcionActual: obtenerSuscripcionActual,
        suscribir: suscribir,
        desuscribir: desuscribir
    };
})();
