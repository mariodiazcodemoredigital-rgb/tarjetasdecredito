namespace TarjetasCredito.Client.Services;

/// <summary>
/// Calcula el último corte de una tarjeta a partir de su día de corte — ver SPEC-005 "Filtro de periodo
/// y paginación" (Historial de compras). Duplica a propósito
/// <c>Domain/Reglas/CicloFacturacionHelper.UltimoCorte</c>: el Client no puede depender de Domain
/// (capas separadas, ver SPEC-001), mismo criterio ya usado para `Domain/Reglas/FechaTextoHelper.cs`.
/// </summary>
public static class CicloFacturacionCliente
{
    public static DateTime UltimoCorte(int diaCorte, DateTime fecha)
    {
        var corteEsteMes = new DateTime(fecha.Year, fecha.Month, DiaValidoEnMes(fecha.Year, fecha.Month, diaCorte));
        if (corteEsteMes <= fecha.Date)
        {
            return corteEsteMes;
        }

        var mesAnterior = fecha.AddMonths(-1);
        return new DateTime(mesAnterior.Year, mesAnterior.Month, DiaValidoEnMes(mesAnterior.Year, mesAnterior.Month, diaCorte));
    }

    private static int DiaValidoEnMes(int year, int month, int dia) => Math.Min(dia, DateTime.DaysInMonth(year, month));
}
