using Microsoft.JSInterop;

namespace TarjetasCredito.Client.Services;

/// <summary>
/// Guarda el refresh token en localStorage (persiste entre sesiones); el access token vive solo en memoria
/// (ver SPEC-004) para reducir la ventana de exposición si el localStorage fuera comprometido.
/// </summary>
public class TokenStorageService(IJSRuntime js)
{
    private const string RefreshTokenKey = "tarjetascredito.refreshToken";

    public string? AccessToken { get; private set; }

    public void SetAccessToken(string? token) => AccessToken = token;

    public async Task GuardarRefreshTokenAsync(string refreshToken)
        => await js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);

    public async Task<string?> ObtenerRefreshTokenAsync()
        => await js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);

    public async Task LimpiarAsync()
    {
        AccessToken = null;
        await js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
    }
}
