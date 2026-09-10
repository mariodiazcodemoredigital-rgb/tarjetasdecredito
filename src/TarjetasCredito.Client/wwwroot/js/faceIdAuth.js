// FaceID/WebAuthn (passkeys) — ver docs/specs/SPEC_TarjetasCredito-004-Seguridad.md "FaceID / biometría".
// Las opciones que manda el servidor y la credencial que produce el navegador viajan como JSON crudo
// en ambas direcciones: nos apoyamos en PublicKeyCredential.parseCreationOptionsFromJSON()/
// .parseRequestOptionsFromJSON() y credential.toJSON() (WebAuthn nivel 3) para no tener que convertir
// ArrayBuffer↔base64 a mano en este módulo.
window.faceIdAuth = (function () {
    // Tope propio (no depender de cuánto tarde el navegador/SO en darse por vencido): en algunos
    // móviles, si el dispositivo no tiene ninguna credencial registrada, navigator.credentials.get()
    // puede tardar mucho más que esto antes de rechazar la promesa por su cuenta — bug real reportado
    // probando en celular (2026-09-10): "el mensaje de que no tenemos Face ID tarda demasiado en salir".
    const TIMEOUT_MS = 15000;

    function conTimeout(promesa) {
        return Promise.race([
            promesa,
            new Promise((_, reject) => setTimeout(() => reject(new Error('timeout')), TIMEOUT_MS))
        ]);
    }

    function disponible() {
        return !!(window.PublicKeyCredential && PublicKeyCredential.parseCreationOptionsFromJSON && PublicKeyCredential.parseRequestOptionsFromJSON);
    }

    async function disponiblePlataforma() {
        if (!disponible()) {
            return false;
        }
        try {
            return await PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
        } catch {
            return false;
        }
    }

    async function registrar(optionsJson) {
        const options = PublicKeyCredential.parseCreationOptionsFromJSON(JSON.parse(optionsJson));
        const credential = await conTimeout(navigator.credentials.create({ publicKey: options }));
        return JSON.stringify(credential.toJSON());
    }

    async function iniciarSesion(optionsJson) {
        const options = PublicKeyCredential.parseRequestOptionsFromJSON(JSON.parse(optionsJson));
        const credential = await conTimeout(navigator.credentials.get({ publicKey: options }));
        return JSON.stringify(credential.toJSON());
    }

    return {
        disponible: disponible,
        disponiblePlataforma: disponiblePlataforma,
        registrar: registrar,
        iniciarSesion: iniciarSesion
    };
})();
