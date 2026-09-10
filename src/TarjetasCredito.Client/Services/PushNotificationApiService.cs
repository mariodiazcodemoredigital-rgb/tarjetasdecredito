using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class PushNotificationApiService(HttpClient http)
{
    public Task<ResultadoApi<VapidClavePublicaDto>> ObtenerClavePublicaAsync()
        => ApiCallHelper.EjecutarAsync(async () =>
            await http.GetFromJsonAsync<VapidClavePublicaDto>("api/pushsubscriptions/vapid-public-key", JsonDefaults.Options)
                ?? throw new InvalidOperationException("Respuesta vacía del servidor."));

    public async Task<HttpResponseMessage> SuscribirAsync(SuscripcionPushRequest request)
        => await http.PostAsJsonAsync("api/pushsubscriptions", request, JsonDefaults.Options);

    public Task<HttpResponseMessage> DesuscribirAsync(string endpoint)
        => http.DeleteAsync($"api/pushsubscriptions?endpoint={Uri.EscapeDataString(endpoint)}");

    public Task<HttpResponseMessage> EnviarPruebaAsync()
        => http.PostAsync("api/pushsubscriptions/prueba", null);
}
