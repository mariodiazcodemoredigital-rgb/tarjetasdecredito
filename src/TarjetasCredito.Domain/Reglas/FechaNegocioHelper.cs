namespace TarjetasCredito.Domain.Reglas;

// "Hoy" para lógica de negocio basada en día calendario (corte, urgencia de vencimiento,
// disparo de notificaciones). Ver SPEC-003 "Zona horaria de negocio: México, no UTC crudo" —
// DateTime.UtcNow directo produce el día calendario equivocado durante buena parte de la
// tarde/noche en México, porque UTC ya cruzó a mañana antes que la hora local.
public static class FechaNegocioHelper
{
    private static readonly TimeZoneInfo ZonaMexico = ResolverZonaMexico();

    public static DateTime AhoraMexico() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);

    private static TimeZoneInfo ResolverZonaMexico()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        // Último recurso: offset fijo UTC-6. México no observa horario de verano desde 2022,
        // así que un offset fijo es correcto, no solo una aproximación.
        return TimeZoneInfo.CreateCustomTimeZone(
            "Mexico-UTC-6-Fallback",
            TimeSpan.FromHours(-6),
            "México (fijo, sin tzdata)",
            "México (fijo, sin tzdata)");
    }
}
