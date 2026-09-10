using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

/// <summary>
/// Ver SPEC-004 "FaceID / biometría" y SPEC-001 "Endpoints de passkeys". Las 4 llamadas de la
/// ceremonia WebAuthn (opciones + registro/acceso) necesitan viajar con credenciales: Identity
/// correlaciona el reto entre esas dos peticiones con una cookie efímera, y Client/Server viven en
/// orígenes distintos — sin `BrowserRequestCredentials.Include` esa cookie nunca hace el viaje de
/// vuelta y el servidor truena con "No passkey attestation/assertion is underway" (bug real corregido
/// 2026-09-09, ver SPEC-004). Listar/eliminar no participan de esa ceremonia, no lo necesitan.
/// </summary>
public class PasskeyApiService(HttpClient http)
{
    private static HttpRequestMessage ConCredenciales(HttpMethod metodo, string url)
    {
        var request = new HttpRequestMessage(metodo, url);
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }

    public async Task<string> ObtenerOpcionesRegistroAsync()
    {
        var response = await http.SendAsync(ConCredenciales(HttpMethod.Post, "api/auth/passkey/registro/opciones"));
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<HttpResponseMessage> RegistrarAsync(string credentialJson)
    {
        var request = ConCredenciales(HttpMethod.Post, "api/auth/passkey/registro");
        request.Content = JsonContent.Create(new PasskeyCredentialRequest(credentialJson), options: JsonDefaults.Options);
        return await http.SendAsync(request);
    }

    public async Task<string> ObtenerOpcionesAccesoAsync()
    {
        var response = await http.SendAsync(ConCredenciales(HttpMethod.Post, "api/auth/passkey/acceso/opciones"));
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<HttpResponseMessage> IniciarSesionAsync(string credentialJson)
    {
        var request = ConCredenciales(HttpMethod.Post, "api/auth/passkey/acceso");
        request.Content = JsonContent.Create(new PasskeyCredentialRequest(credentialJson), options: JsonDefaults.Options);
        return await http.SendAsync(request);
    }

    public Task<ResultadoApi<List<PasskeyInfoDto>>> ListarAsync()
        => ApiCallHelper.EjecutarAsync(async () =>
            await http.GetFromJsonAsync<List<PasskeyInfoDto>>("api/auth/passkey", JsonDefaults.Options) ?? []);

    public Task<HttpResponseMessage> EliminarAsync(string credentialId)
        => http.DeleteAsync($"api/auth/passkey/{credentialId}");
}
