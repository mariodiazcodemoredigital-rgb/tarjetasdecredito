// Utilidades DOM mínimas de uso general — ver SPEC-005 "Editar desplaza el foco al formulario".
window.domUtils = {
    scrollIntoView(id) {
        const el = document.getElementById(id);
        if (el) {
            el.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }
};
