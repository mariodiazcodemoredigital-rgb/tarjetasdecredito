using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class CreditCardApiService(HttpClient http)
{
    public Task<ResultadoApi<List<CreditCardDto>>> ObtenerTodasAsync()
        => ApiCallHelper.EjecutarAsync(async () =>
            await http.GetFromJsonAsync<List<CreditCardDto>>("api/creditcards", JsonDefaults.Options) ?? []);

    public async Task<HttpResponseMessage> CrearAsync(CrearCreditCardRequest request)
        => await http.PostAsJsonAsync("api/creditcards", request, JsonDefaults.Options);

    public async Task<HttpResponseMessage> ActualizarAsync(Guid id, CrearCreditCardRequest request)
        => await http.PutAsJsonAsync($"api/creditcards/{id}", request, JsonDefaults.Options);

    public Task<HttpResponseMessage> DesactivarAsync(Guid id)
        => http.DeleteAsync($"api/creditcards/{id}");
}
