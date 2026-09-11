namespace TarjetasCredito.Domain.Reglas;

/// <summary>
/// Formatea fechas en español ("03 Octubre 2026") para textos generados en Domain (ej. la explicación
/// de <see cref="MotorRecomendacionTarjeta"/>) — ver SPEC-005 "Formato de fecha". Duplica a propósito la
/// tabla de meses de <c>TarjetasCredito.Client.Services.FechaFormato</c>: Domain no puede depender de
/// Client (capas separadas, ver SPEC-001) y es la única forma de que un texto armado en Domain use el
/// mismo formato de fecha que el resto de la UI, sin acoplar capas que deben quedar independientes.
/// </summary>
public static class FechaTextoHelper
{
    private static readonly string[] Meses =
    [
        "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    ];

    public static string Corta(DateTime fecha) => $"{fecha.Day:00} {Meses[fecha.Month - 1]} {fecha.Year}";
}
