namespace TarjetasCredito.Client.Services;

/// <summary>Ver SPEC-001, sección "Manejo de errores de red y estados sin conexión".</summary>
public static class ApiCallHelper
{
    public static async Task<ResultadoApi<T>> EjecutarAsync<T>(Func<Task<T>> llamada)
    {
        try
        {
            return ResultadoApi<T>.Ok(await llamada());
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null)
        {
            // No llegó respuesta del servidor: sin red, DNS, conexión rechazada, etc.
            return ResultadoApi<T>.Falla(sinConexion: true);
        }
        catch (TaskCanceledException)
        {
            // Timeout del HttpClient: se trata igual que sin conexión.
            return ResultadoApi<T>.Falla(sinConexion: true);
        }
        catch (HttpRequestException)
        {
            // Sí hubo respuesta del servidor, pero de error (ej. 500).
            return ResultadoApi<T>.Falla(sinConexion: false);
        }
    }
}
