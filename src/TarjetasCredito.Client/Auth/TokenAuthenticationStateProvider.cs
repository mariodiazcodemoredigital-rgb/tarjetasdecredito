using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TarjetasCredito.Client.Services;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Auth;

/// <summary>
/// El access token que emite MapIdentityApi es un token opaco cifrado (Data Protection), NO un JWT —
/// no tiene tres segmentos separados por punto y no se puede decodificar en el cliente. Por eso el
/// ClaimsPrincipal para la UI se construye consultando /api/profile ("quién soy") con el token ya
/// adjunto como bearer, en vez de parsear el token.
/// </summary>
public class TokenAuthenticationStateProvider(HttpClient http, TokenStorageService tokenStorage) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonimo = new(new ClaimsIdentity());
    private ClaimsPrincipal principal = Anonimo;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(principal));

    public async Task<bool> IntentarRestaurarSesionAsync()
    {
        var refreshToken = await tokenStorage.ObtenerRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        try
        {
            var response = await http.PostAsJsonAsync("api/auth/refresh", new { refreshToken }, JsonDefaults.Options);
            if (!response.IsSuccessStatusCode)
            {
                await tokenStorage.LimpiarAsync();
                return false;
            }

            var tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(JsonDefaults.Options);
            if (tokens is null)
            {
                return false;
            }

            await AplicarSesionAsync(tokens);
            return principal.Identity?.IsAuthenticated ?? false;
        }
        catch (Exception)
        {
            // Servidor inalcanzable (sin conexión, API caída, etc.): se trata como sesión no restaurada
            // en vez de tumbar el arranque de la app — el usuario simplemente ve el login.
            return false;
        }
    }

    public async Task AplicarSesionAsync(AccessTokenResponse tokens)
    {
        tokenStorage.SetAccessToken(tokens.AccessToken);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        await tokenStorage.GuardarRefreshTokenAsync(tokens.RefreshToken);
        await CargarPrincipalAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task CerrarSesionAsync()
    {
        await tokenStorage.LimpiarAsync();
        http.DefaultRequestHeaders.Authorization = null;
        principal = Anonimo;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task CargarPrincipalAsync()
    {
        try
        {
            var perfil = await http.GetFromJsonAsync<UserProfileDto>("api/profile", JsonDefaults.Options);
            if (perfil is not null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, perfil.Email),
                    new(ClaimTypes.Email, perfil.Email)
                };
                principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "apiauth"));
                return;
            }
        }
        catch
        {
            // token inválido/expirado o sin conexión: se trata como no autenticado
        }

        principal = Anonimo;
    }
}
