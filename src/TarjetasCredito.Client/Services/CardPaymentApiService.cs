using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class CardPaymentApiService(HttpClient http)
{
    public Task<ResultadoApi<List<CardPaymentDto>>> ObtenerTodosAsync()
        => ApiCallHelper.EjecutarAsync(async () =>
            await http.GetFromJsonAsync<List<CardPaymentDto>>("api/cardpayments", JsonDefaults.Options) ?? []);

    public Task<HttpResponseMessage> CrearAsync(CrearCardPaymentRequest request)
        => http.PostAsJsonAsync("api/cardpayments", request, JsonDefaults.Options);

    public Task<HttpResponseMessage> ActualizarAsync(Guid id, CrearCardPaymentRequest request)
        => http.PutAsJsonAsync($"api/cardpayments/{id}", request, JsonDefaults.Options);

    public Task<HttpResponseMessage> EliminarAsync(Guid id)
        => http.DeleteAsync($"api/cardpayments/{id}");
}
