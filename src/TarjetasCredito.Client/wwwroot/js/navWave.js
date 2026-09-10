// Patrón reutilizable "navegación wave" (pill elástica): un fondo con degradado se desliza con rebote
// detrás del ítem de nav con hover/foco/ruta activa. Ver docs/specs/SPEC_TarjetasCredito-005-UI.md →
// "Patrón reutilizable: navegación 'wave' (pill elástica)" para la guía de uso.
// init(navSelector, itemSelector, orientation) — orientation: 'horizontal' (.bottom-nav) o 'vertical' (.side-nav).
// Mismo esquema que parallaxStage.js: el JS solo escribe variables CSS, la transición vive en CSS.
// init() es idempotente: quien lo llama (MainLayout) puede invocarlo en cada render sin problema —
// si el nav no cambió desde la última vez, no hace nada; si Blazor reemplazó el nodo del DOM
// (ver bug real más abajo), vuelve a enlazar todo automáticamente.
window.navWave = (function () {
    const activos = new Map();

    function init(navSelector, itemSelector, orientation) {
        itemSelector = itemSelector || 'a';
        orientation = orientation === 'vertical' ? 'vertical' : 'horizontal';

        const nav = document.querySelector(navSelector);
        if (!nav) {
            // El nav todavía no existe en el DOM (ej. AuthorizeView no ha resuelto el estado de
            // autenticación en el primer render) — quien llama debe reintentar en el próximo render.
            return false;
        }

        const existente = activos.get(navSelector);
        if (existente && existente.nav === nav) {
            // Ya está enlazado a este mismo nodo del DOM — nada que hacer.
            return true;
        }
        // Bug real (2026-09-09): Blazor puede reemplazar el <nav> completo (ej. cuando el perfil del
        // usuario termina de cargar y se re-renderiza el bloque <Authorized>) DESPUÉS de que ya nos
        // habíamos enlazado a los <a> originales — esos nodos quedan desconectados del DOM y sus
        // listeners dejan de tener efecto visible, aunque `init()` ya había devuelto `true` antes.
        // Por eso `init()` no se limita a "enlazar una sola vez": compara el nodo actual contra el
        // último enlazado y vuelve a enlazar si cambió, en vez de confiar en una bandera de "ya listo".
        if (existente) {
            existente.cleanup();
        }

        const pill = nav.querySelector('.nav-wave-pill');
        const items = Array.from(nav.querySelectorAll(itemSelector));
        if (!pill || items.length === 0) {
            return false;
        }

        const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
        if (reducedMotion.matches) {
            nav.classList.add('wave-sin-movimiento');
        }

        function moverPillA(item) {
            const navRect = nav.getBoundingClientRect();
            const itemRect = item.getBoundingClientRect();
            if (orientation === 'vertical') {
                pill.style.setProperty('--wave-y', (itemRect.top - navRect.top) + 'px');
                pill.style.setProperty('--wave-h', itemRect.height + 'px');
            } else {
                pill.style.setProperty('--wave-x', (itemRect.left - navRect.left) + 'px');
                pill.style.setProperty('--wave-w', itemRect.width + 'px');
            }
            return itemRect.width > 0 && itemRect.height > 0;
        }

        // El ítem con la pill detrás siempre queda marcado "wave-activo" (color de contraste + elevación) —
        // tanto al pasar el mouse como al "asentarse" sobre la ruta activa (ver volverAActivo) — así el
        // efecto es uno solo y continuo: el anterior "baja" (pierde wave-activo) mientras el nuevo "sube".
        function activarItem(item) {
            items.forEach((otro) => otro.classList.toggle('wave-activo', otro === item));
            return moverPillA(item);
        }

        function itemActivo() {
            return items.find((item) => item.classList.contains('active')) || null;
        }

        function volverAActivo() {
            const actual = itemActivo();
            if (actual) {
                return activarItem(actual);
            }
            items.forEach((item) => item.classList.remove('wave-activo'));
            return true;
        }

        // Se escucha tanto "pointerenter" (mouse/touch/pen moderno) como "mouseenter" (siempre soportado,
        // incluida cualquier herramienta/entorno que solo simule eventos de mouse clásicos) — ambos llaman
        // al mismo manejador, sin costo por que se disparen los dos en un mouse real.
        const handlers = items.map((item) => {
            const onEnter = () => activarItem(item);
            const onFocus = () => activarItem(item);
            item.addEventListener('pointerenter', onEnter);
            item.addEventListener('mouseenter', onEnter);
            item.addEventListener('focusin', onFocus);
            return { item, onEnter, onFocus };
        });
        nav.addEventListener('pointerleave', volverAActivo);
        nav.addEventListener('mouseleave', volverAActivo);

        // Reintenta hasta obtener una medida real (ancho/alto > 0) o agotar los intentos, en vez de
        // confiar en un solo intento — justo al patchear el DOM, o al cruzar el breakpoint móvil/escritorio
        // en un resize, el layout puede no estar asentado todavía y getBoundingClientRect() de los <a>
        // devuelve 0 o valores transitorios incorrectos (dos bugs reales, ver SPEC-005).
        function asentarPill(intentosRestantes) {
            const lista = volverAActivo();
            if (!lista && intentosRestantes > 0) {
                setTimeout(function () { asentarPill(intentosRestantes - 1); }, 50);
            }
        }

        // Posición inicial: sin animación, directo sobre la ruta activa (si "vuela" desde 0,0 se ve mal).
        nav.classList.add('wave-sin-movimiento');
        setTimeout(function () {
            asentarPill(20);
            // Fuerza reflow antes de reactivar la transición para que la colocación inicial no se anime.
            void pill.offsetWidth;
            if (!reducedMotion.matches) {
                nav.classList.remove('wave-sin-movimiento');
            }
        }, 0);

        // En resize (incluido cruzar el breakpoint móvil/escritorio, que cambia display:none <-> flex
        // de este nav) se reintenta igual que en el arranque, con un pequeño debounce: el navegador puede
        // disparar varios "resize" seguidos mientras el usuario arrastra el borde de la ventana.
        let resizeTimeout;
        function onResize() {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(function () { asentarPill(20); }, 120);
        }
        window.addEventListener('resize', onResize);

        // Blazor alterna la clase "active" en el <a> correspondiente al navegar (SPA, sin reload) —
        // este observer detecta ese cambio y desliza la pill hacia la nueva ruta aunque no haya hover
        // (ej. redirección programática, o clic sin que el mouse termine sobre el nuevo ítem).
        const observer = new MutationObserver(() => {
            const actual = itemActivo();
            if (actual && !actual.classList.contains('wave-activo')) {
                activarItem(actual);
            }
        });
        items.forEach((item) => observer.observe(item, { attributes: true, attributeFilter: ['class'] }));

        activos.set(navSelector, {
            nav: nav,
            cleanup: function () {
                handlers.forEach(({ item, onEnter, onFocus }) => {
                    item.removeEventListener('pointerenter', onEnter);
                    item.removeEventListener('mouseenter', onEnter);
                    item.removeEventListener('focusin', onFocus);
                });
                nav.removeEventListener('pointerleave', volverAActivo);
                nav.removeEventListener('mouseleave', volverAActivo);
                window.removeEventListener('resize', onResize);
                clearTimeout(resizeTimeout);
                observer.disconnect();
            }
        });

        return true;
    }

    function detener(navSelector) {
        const existente = activos.get(navSelector);
        if (existente) {
            existente.cleanup();
            activos.delete(navSelector);
        }
    }

    return { init: init, detener: detener };
})();
