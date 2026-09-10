// Wrapper de SweetAlert2 para la app (ver docs/specs/SPEC_TarjetasCredito-005-UI.md).
// Lee las variables CSS del tema activo para que los diálogos/toasts se vean acordes al claro/oscuro.
window.appDialogs = (function () {
    function css(varName) {
        return getComputedStyle(document.documentElement).getPropertyValue(varName).trim();
    }

    function baseSwal() {
        return Swal.mixin({
            background: css('--surface'),
            color: css('--text'),
            confirmButtonColor: css('--primary'),
            cancelButtonColor: css('--surface-2'),
            customClass: {
                popup: 'app-swal-popup',
                confirmButton: 'app-swal-btn',
                cancelButton: 'app-swal-btn app-swal-btn-cancel'
            },
            buttonsStyling: false
        });
    }

    async function confirmar(titulo, texto, textoConfirmar, icono) {
        const esAdvertencia = (icono || 'warning') === 'warning';
        const result = await baseSwal().fire({
            title: titulo,
            text: texto,
            icon: icono || 'warning',
            iconColor: esAdvertencia ? css('--danger') : css('--primary'),
            showCancelButton: true,
            confirmButtonText: textoConfirmar || 'Confirmar',
            cancelButtonText: 'Cancelar',
            reverseButtons: true
        });
        return result.isConfirmed;
    }

    function toast(tipo, mensaje) {
        baseSwal().mixin({
            toast: true,
            position: 'top-end',
            timer: 3500,
            timerProgressBar: true,
            showConfirmButton: false,
            customClass: {
                popup: 'app-swal-toast'
            }
        }).fire({
            icon: tipo,
            iconColor: tipo === 'error' ? css('--danger') : css('--success'),
            title: mensaje
        });
    }

    return {
        confirmarEliminar: function (titulo, texto, textoConfirmar) { return confirmar(titulo, texto, textoConfirmar, 'warning'); },
        confirmar: confirmar,
        toastError: function (mensaje) { toast('error', mensaje); },
        toastExito: function (mensaje) { toast('success', mensaje); }
    };
})();
