using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class PurchaseApiService(HttpClient http)
{
    public Task<ResultadoApi<List<PurchaseDto>>> ObtenerTodasAsync(Guid? creditCardId = null)
    {
        var url = creditCardId is null ? "api/purchases" : $"api/purchases?creditCardId={creditCardId}";
        return ApiCallHelper.EjecutarAsync(async () =>
            await http.GetFromJsonAsync<List<PurchaseDto>>(url, JsonDefaults.Options) ?? []);
    }

    public Task<ResultadoApi<List<string>>> ObtenerCategoriasAsync()
        => ApiCallHelper.EjecutarAsync(async () =>
            await http.GetFromJsonAsync<List<string>>("api/purchases/categorias", JsonDefaults.Options) ?? []);

    public Task<HttpResponseMessage> CrearAsync(CrearPurchaseRequest request)
        => http.PostAsJsonAsync("api/purchases", request, JsonDefaults.Options);

    public Task<HttpResponseMessage> ActualizarAsync(Guid id, CrearPurchaseRequest request)
        => http.PutAsJsonAsync($"api/purchases/{id}", request, JsonDefaults.Options);

    public Task<HttpResponseMessage> EliminarAsync(Guid id)
        => http.DeleteAsync($"api/purchases/{id}");
}
