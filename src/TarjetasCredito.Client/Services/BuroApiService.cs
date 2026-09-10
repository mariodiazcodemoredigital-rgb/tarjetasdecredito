using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class BuroApiService(HttpClient http)
{
    public Task<ResultadoApi<BuroScoreResponse?>> ObtenerScoreAsync()
        => ApiCallHelper.EjecutarAsync(() => http.GetFromJsonAsync<BuroScoreResponse>("api/buro/score", JsonDefaults.Options));
}
