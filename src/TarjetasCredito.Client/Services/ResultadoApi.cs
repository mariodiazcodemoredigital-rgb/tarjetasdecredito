namespace TarjetasCredito.Client.Services;

public record ResultadoApi<T>(T? Datos, bool SinConexion, bool ErrorServidor)
{
    public bool Exitoso => !SinConexion && !ErrorServidor;

    public static ResultadoApi<T> Ok(T datos) => new(datos, false, false);

    public static ResultadoApi<T> Falla(bool sinConexion) => new(default, sinConexion, !sinConexion);
}
