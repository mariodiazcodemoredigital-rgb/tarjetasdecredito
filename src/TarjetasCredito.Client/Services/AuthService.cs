using System.Net.Http.Json;
using Microsoft.JSInterop;
using TarjetasCredito.Client.Auth;

namespace TarjetasCredito.Client.Services;

public record ResultadoAuth(bool Exitoso, IReadOnlyList<string> Errores);

public class AuthService(
    HttpClient http,
    TokenAuthenticationStateProvider authStateProvider,
    UserProfileService userProfileService,
    PasskeyApiService passkeyApi,
    IJSRuntime js)
{
    public async Task<ResultadoAuth> RegistrarAsync(string email, string password)
    {
        try
        {
            var response = await http.PostAsJsonAsync("api/auth/register", new RegisterRequest(email, password), JsonDefaults.Options);
            if (response.IsSuccessStatusCode)
            {
                return new ResultadoAuth(true, []);
            }

            return new ResultadoAuth(false, await ExtraerErroresAsync(response));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ResultadoAuth(false, ["No se pudo conectar. Revisa tu conexión a internet."]);
        }
    }

    public async Task<ResultadoAuth> IniciarSesionAsync(string email, string password)
    {
        try
        {
            var response = await http.PostAsJsonAsync("api/auth/login?useCookies=false", new LoginRequest(email, password), JsonDefaults.Options);
            if (!response.IsSuccessStatusCode)
            {
                // A diferencia de Registrar, un login fallido de MapIdentityApi no devuelve errores de
                // IdentityErrorDescriber (nunca pasa por DescriptorErroresIdentityEspanol) — es un
                // ProblemDetails genérico cuyo Title/Detail default de ASP.NET Core viene en inglés
                // ("Unauthorized"/"Failed"). Se usa un mensaje fijo en español en vez de reenviarlo, lo
                // que además evita revelar si el correo existe o la contraseña es la que está mal.
                return new ResultadoAuth(false, ["Correo o contraseña incorrectos."]);
            }

            var tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(JsonDefaults.Options);
            if (tokens is null)
            {
                return new ResultadoAuth(false, ["No se pudo interpretar la respuesta del servidor."]);
            }

            await authStateProvider.AplicarSesionAsync(tokens);
            await userProfileService.CargarAsync();
            return new ResultadoAuth(true, []);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ResultadoAuth(false, ["No se pudo conectar. Revisa tu conexión a internet."]);
        }
    }

    /// <summary>
    /// Login "discoverable" con passkey: el navegador muestra su propio selector de cuenta, sin pedir
    /// correo primero. Ver SPEC-004 "FaceID / biometría".
    /// </summary>
    public async Task<ResultadoAuth> IniciarSesionConFaceIdAsync()
    {
        try
        {
            var opciones = await passkeyApi.ObtenerOpcionesAccesoAsync();

            string credencial;
            try
            {
                credencial = await js.InvokeAsync<string>("faceIdAuth.iniciarSesion", opciones);
            }
            catch (JSException)
            {
                // El usuario canceló el prompt biométrico o no hay ninguna passkey en este dispositivo.
                return new ResultadoAuth(false, ["No se pudo verificar con Face ID. Intenta de nuevo."]);
            }

            var response = await passkeyApi.IniciarSesionAsync(credencial);
            if (!response.IsSuccessStatusCode)
            {
                return new ResultadoAuth(false, ["No se pudo iniciar sesión con Face ID."]);
            }

            var tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(JsonDefaults.Options);
            if (tokens is null)
            {
                return new ResultadoAuth(false, ["No se pudo interpretar la respuesta del servidor."]);
            }

            await authStateProvider.AplicarSesionAsync(tokens);
            await userProfileService.CargarAsync();
            return new ResultadoAuth(true, []);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ResultadoAuth(false, ["No se pudo conectar. Revisa tu conexión a internet."]);
        }
    }

    public async Task CerrarSesionAsync()
    {
        await authStateProvider.CerrarSesionAsync();
        userProfileService.Limpiar();
    }

    private static async Task<IReadOnlyList<string>> ExtraerErroresAsync(HttpResponseMessage response)
    {
        try
        {
            var problema = await response.Content.ReadFromJsonAsync<IdentityErrorResponse>(JsonDefaults.Options);
            if (problema?.Errors is { Count: > 0 })
            {
                // Estos SÍ están garantizados en español: vienen de IdentityResult.Errors, armados por
                // DescriptorErroresIdentityEspanol (ver SPEC-005). No se usa problema.Title como
                // fallback — es el ProblemDetails genérico de ASP.NET Core (reason-phrase del status
                // code, en inglés), el mismo bug real ya corregido en IniciarSesionAsync.
                return problema.Errors.SelectMany(e => e.Value).ToList();
            }
        }
        catch
        {
            // el cuerpo no era el formato de error esperado; se usa el mensaje genérico
        }

        return [$"No se pudo completar la operación ({(int)response.StatusCode})."];
    }
}
