// Máscara de moneda para <input type="text"> — ver SPEC-005 "Máscara de moneda". Formatea en vivo con
// separador de miles (coma) mientras se escribe, preservando la posición del cursor; en blur normaliza
// a 2 decimales. Usa Number.toLocaleString('en-US') a propósito (no depende de la cultura de .NET WASM,
// que ya causó el bug de coma decimal documentado en SPEC-005 "Formato de fecha").
window.currencyMask = {
    attach(el) {
        if (el.dataset.currencyMaskAttached) {
            return;
        }
        el.dataset.currencyMaskAttached = "1";
        el.addEventListener("input", () => this._reformatEnVivo(el));
        el.addEventListener("blur", () => this._reformatFinal(el));
    },

    _crudo(valor) {
        let v = valor.replace(/[^\d.]/g, "");
        const primerPunto = v.indexOf(".");
        if (primerPunto !== -1) {
            v = v.slice(0, primerPunto + 1) + v.slice(primerPunto + 1).replace(/\./g, "");
        }
        const partes = v.split(".");
        if (partes.length === 2) {
            v = partes[0] + "." + partes[1].slice(0, 2);
        }
        return v;
    },

    _reformatEnVivo(el) {
        const antes = el.value;
        const cursorAntes = el.selectionStart ?? antes.length;
        const digitosAntesDelCursor = (antes.slice(0, cursorAntes).match(/[\d.]/g) || []).length;

        const crudo = this._crudo(antes);
        const [parteEntera, parteDecimal] = crudo.split(".");
        const enteraFormateada = parteEntera === "" ? "" : Number(parteEntera).toLocaleString("en-US");
        const formateado = parteDecimal !== undefined ? `${enteraFormateada}.${parteDecimal}` : enteraFormateada;

        el.value = formateado;

        let contados = 0;
        let posicion = formateado.length;
        for (let i = 0; i < formateado.length; i++) {
            if (/[\d.]/.test(formateado[i])) {
                contados++;
            }
            if (contados >= digitosAntesDelCursor) {
                posicion = i + 1;
                break;
            }
        }
        el.setSelectionRange(posicion, posicion);
    },

    _reformatFinal(el) {
        const crudo = this._crudo(el.value);
        if (crudo === "" || crudo === ".") {
            el.value = "";
            return;
        }
        const numero = parseFloat(crudo);
        if (Number.isNaN(numero)) {
            el.value = "";
            return;
        }
        el.value = numero.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
};
