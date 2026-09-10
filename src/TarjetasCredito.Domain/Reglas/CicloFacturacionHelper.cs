namespace TarjetasCredito.Domain.Reglas;

/// <summary>Calcula fechas de corte a partir del día de corte configurado en la tarjeta (ver SPEC-003).</summary>
public static class CicloFacturacionHelper
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

    public static DateTime ProximoCorte(int diaCorte, DateTime fecha)
    {
        var ultimo = UltimoCorte(diaCorte, fecha);
        var siguienteMes = ultimo.AddMonths(1);
        return new DateTime(siguienteMes.Year, siguienteMes.Month, DiaValidoEnMes(siguienteMes.Year, siguienteMes.Month, diaCorte));
    }

    /// <summary>
    /// Los dos ciclos que se deben tener como <see cref="Entities.PaymentReminder"/>: el vigente y el
    /// siguiente (ver SPEC-003 "Generación y gestión de recordatorios de pago"). Función pura — quien la
    /// llama decide qué hacer con los ciclos (ej. crear el recordatorio si no existe ya).
    /// </summary>
    public static IReadOnlyList<(DateTime CicloInicio, DateTime CicloFin)> CiclosARecordar(int diaCorte, DateTime fecha)
    {
        var cicloFinVigente = ProximoCorte(diaCorte, fecha);
        var cicloInicioVigente = UltimoCorte(diaCorte, fecha);
        var cicloFinSiguiente = ProximoCorte(diaCorte, cicloFinVigente.AddDays(1));

        return
        [
            (cicloInicioVigente, cicloFinVigente),
            (cicloFinVigente, cicloFinSiguiente)
        ];
    }

    private static int DiaValidoEnMes(int year, int month, int dia) => Math.Min(dia, DateTime.DaysInMonth(year, month));
}
