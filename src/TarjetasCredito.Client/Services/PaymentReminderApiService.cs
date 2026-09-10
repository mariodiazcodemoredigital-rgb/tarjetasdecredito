using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class PaymentReminderApiService(HttpClient http)
{
    public Task<ResultadoApi<List<PaymentReminderDto>>> ObtenerTodosAsync()
        => ApiCallHelper.EjecutarAsync(async () =>
            await http.GetFromJsonAsync<List<PaymentReminderDto>>("api/paymentreminders", JsonDefaults.Options) ?? []);

    public Task<HttpResponseMessage> MarcarPagadoAsync(Guid id, decimal? montoEstadoCuenta)
        => http.PutAsJsonAsync($"api/paymentreminders/{id}/pagado", new MarcarPagadoRequest(montoEstadoCuenta, null), JsonDefaults.Options);
}
