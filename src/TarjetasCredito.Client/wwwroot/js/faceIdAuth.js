// FaceID/WebAuthn (passkeys) — ver docs/specs/SPEC_TarjetasCredito-004-Seguridad.md "FaceID / biometría".
// Las opciones que manda el servidor y la credencial que produce el navegador viajan como JSON crudo
// en ambas direcciones: nos apoyamos en PublicKeyCredential.parseCreationOptionsFromJSON()/
// .parseRequestOptionsFromJSON() y credential.toJSON() (WebAuthn nivel 3) para no tener que convertir
// ArrayBuffer↔base64 a mano en este módulo.
window.faceIdAuth = (function () {
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
        const credential = await navigator.credentials.create({ publicKey: options });
        return JSON.stringify(credential.toJSON());
    }

    async function iniciarSesion(optionsJson) {
        const options = PublicKeyCredential.parseRequestOptionsFromJSON(JSON.parse(optionsJson));
        const credential = await navigator.credentials.get({ publicKey: options });
        return JSON.stringify(credential.toJSON());
    }

    return {
        disponible: disponible,
        disponiblePlataforma: disponiblePlataforma,
        registrar: registrar,
        iniciarSesion: iniciarSesion
    };
})();
