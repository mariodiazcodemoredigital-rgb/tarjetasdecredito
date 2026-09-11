namespace TarjetasCredito.Client.Services;

/// <summary>
/// Formato de fecha en español con el nombre del mes completo ("03 Octubre 2026") — el usuario pidió
/// explícitamente el nombre del mes sin abreviar, en vez de numérico ("03/10/2026") o abreviado
/// ("03 Oct 2026", usado hasta Sprint 39). Ver SPEC-005 "Formato de fecha". No depende de la
/// configuración regional del navegador (evita problemas de globalización en Blazor WASM) — usa una
/// tabla fija de nombres de mes.
/// </summary>
public static class FechaFormato
{
    private static readonly string[] Meses =
    [
        "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    ];

    public static string Corta(DateTime fecha) => $"{fecha.Day:00} {Meses[fecha.Month - 1]} {fecha.Year}";

    /// <summary>Sin año — para fechas cercanas embebidas a mitad de una oración (ej. "Mejora tu score":
    /// "Tu corte es el 12 Septiembre"), donde el año es obvio por contexto y agregarlo sería ruido.</summary>
    public static string CortaSinAnio(DateTime fecha) => $"{fecha.Day:00} {Meses[fecha.Month - 1]}";
}
