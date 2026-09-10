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
    /// El ciclo de facturación vigente (el que contiene "fecha") — el único que se genera como
    /// <see cref="Entities.PaymentReminder"/> (ver SPEC-003 "Generación y gestión de recordatorios de
    /// pago": antes se generaba también el ciclo siguiente por adelantado, decisión revertida a
    /// petición del usuario probando con datos reales — ver "un solo ciclo a la vez" en esa sección).
    /// </summary>
    public static (DateTime CicloInicio, DateTime CicloFin) CicloVigente(int diaCorte, DateTime fecha)
        => (UltimoCorte(diaCorte, fecha), ProximoCorte(diaCorte, fecha));

    private static int DiaValidoEnMes(int year, int month, int dia) => Math.Min(dia, DateTime.DaysInMonth(year, month));
}
