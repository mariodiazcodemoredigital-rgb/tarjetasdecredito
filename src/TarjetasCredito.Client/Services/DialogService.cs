using Microsoft.JSInterop;

namespace TarjetasCredito.Client.Services;

/// <summary>Wrapper de SweetAlert2 (ver wwwroot/js/dialogs.js y SPEC-005).</summary>
public class DialogService(IJSRuntime js)
{
    public ValueTask<bool> ConfirmarEliminarAsync(string titulo, string texto, string? textoConfirmar = null)
        => js.InvokeAsync<bool>("appDialogs.confirmarEliminar", titulo, texto, textoConfirmar);

    /// <summary>Confirmación no destructiva (ícono de pregunta en vez de advertencia), ej. descartar cambios sin guardar.</summary>
    public ValueTask<bool> ConfirmarAsync(string titulo, string texto, string? textoConfirmar = null, string icono = "question")
        => js.InvokeAsync<bool>("appDialogs.confirmar", titulo, texto, textoConfirmar, icono);

    public ValueTask ToastErrorAsync(string mensaje)
        => js.InvokeVoidAsync("appDialogs.toastError", mensaje);

    public ValueTask ToastExitoAsync(string mensaje)
        => js.InvokeVoidAsync("appDialogs.toastExito", mensaje);
}
