using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class RecommendationApiService(HttpClient http)
{
    public async Task<RecomendacionResponse?> RecomendarAsync(decimal monto, DateTime? fecha = null)
    {
        var response = await http.PostAsJsonAsync("api/recommendation", new RecomendacionRequest(monto, fecha), JsonDefaults.Options);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RecomendacionResponse>(JsonDefaults.Options)
            : null;
    }
}
