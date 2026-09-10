// Patrón reutilizable "parallax stage": elementos que se inclinan/desplazan siguiendo el cursor.
// Ver docs/specs/SPEC_TarjetasCredito-005-UI.md → "Patrón reutilizable: parallax stage" para la guía de uso.
// Motor genérico, sin librerías ni assets externos — solo CSS transforms sobre los elementos que le pases.
// init(stageSelector, itemSelector, depthStep?) — depthStep (default 7) controla qué tanto se desplazan los
// elementos por cada unidad de movimiento del cursor; baja este valor en espacios compactos para evitar que
// el desplazamiento choque con contenido vecino.
window.parallaxStage = (function () {
    const activos = new Map();

    function clamp(value, min, max) {
        return Math.min(max, Math.max(min, value));
    }

    function init(stageSelector, itemSelector, depthStep) {
        stageSelector = stageSelector || '.parallax-stage';
        itemSelector = itemSelector || '.parallax-item';
        depthStep = depthStep || 7;

        detener(stageSelector);

        const stage = document.querySelector(stageSelector);
        if (!stage) {
            return;
        }

        const items = Array.from(stage.querySelectorAll(itemSelector));
        if (items.length === 0) {
            return;
        }

        const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
        let frame = 0;

        function apply(x, y, pointerType) {
            const rect = stage.getBoundingClientRect();
            // El cursor puede estar en cualquier parte de la página (el listener es global), muy lejos
            // del stage — sin este tope el desplazamiento crece sin límite y el elemento se sale del
            // stage (y de su overflow:hidden) cuanto más lejos se mueva el cursor. Con el tope, al llegar
            // al borde del stage el elemento alcanza su desplazamiento máximo y ahí se queda: siempre
            // completo, nunca recortado ni "perdido" (bug real corregido 2026-09-09).
            const dx = clamp((x - (rect.left + rect.width / 2)) / (rect.width / 2), -1, 1);
            const dy = clamp((y - (rect.top + rect.height / 2)) / (rect.height / 2), -1, 1);
            // En touch los eventos llegan más espaciados que con mouse — la transición de 0.15s (pensada
            // para suavizar el mouse, que manda muchísimos eventos) hace que se sienta "atrasada" respecto
            // al dedo. En touch quitamos la transición para que el seguimiento sea 1:1 e inmediato; en mouse
            // se conserva la suavidad (bug real corregido 2026-09-09).
            const transitionDuration = pointerType === 'touch' ? '0s' : '';

            items.forEach((item, i) => {
                const depth = (i + 1) * depthStep;
                const rotate = dx * (3 + i * 1.5) * (depthStep / 7);
                item.style.transitionDuration = transitionDuration;
                // Solo el "empujón" del cursor — la posición/rotación base vive en CSS (--base-x/--base-y/--base-r)
                // y se suma vía calc() en la regla .parallax-item, para no perderla al escribir estas variables.
                item.style.setProperty('--px', (dx * depth).toFixed(2) + 'px');
                item.style.setProperty('--py', (dy * depth).toFixed(2) + 'px');
                item.style.setProperty('--pr', rotate.toFixed(2) + 'deg');
            });
        }

        function onMove(e) {
            if (reducedMotion.matches) {
                return;
            }
            const pointerType = e.pointerType;
            if (!frame) {
                frame = requestAnimationFrame(function () {
                    frame = 0;
                    apply(e.clientX, e.clientY, pointerType);
                });
            }
        }

        window.addEventListener('pointermove', onMove, { passive: true });

        activos.set(stageSelector, function () {
            window.removeEventListener('pointermove', onMove);
            cancelAnimationFrame(frame);
        });
    }

    function detener(stageSelector) {
        const cleanup = activos.get(stageSelector);
        if (cleanup) {
            cleanup();
            activos.delete(stageSelector);
        }
    }

    return { init: init, detener: detener };
})();
