namespace TarjetasCredito.Client.Services;

/// <summary>
/// Formato de fecha corto en español ("02 Sep 2026") en vez de numérico ("02/10/2026") — el usuario
/// reportó que el formato numérico era ambiguo/confuso al leerlo rápido en el Dashboard. Ver SPEC-005
/// "Formato de fecha". No depende de la configuración regional del navegador (evita problemas de
/// globalización en Blazor WASM) — usa una tabla fija de abreviaturas.
/// </summary>
public static class FechaFormato
{
    private static readonly string[] MesesCortos =
        ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"];

    public static string Corta(DateTime fecha) => $"{fecha.Day:00} {MesesCortos[fecha.Month - 1]} {fecha.Year}";
}
