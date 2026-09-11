using System.Globalization;

namespace TarjetasCredito.Client.Services;

/// <summary>
/// Máscara de moneda "en vivo" para campos que necesitan reaccionar en cada tecla (ej. habilitar/
/// deshabilitar un botón mientras se escribe) — ver SPEC-005 "Máscara de moneda". A diferencia de
/// <c>Components/InputMonto.razor</c> (que sincroniza solo en blur vía JS, pensado para formularios de
/// registro), este helper formatea directamente en C# en cada tecla — el cursor salta al final del
/// campo en cada tecla, una simplificación aceptable para un monto corto que normalmente se escribe de
/// izquierda a derecha sin necesidad de editar a la mitad.
/// </summary>
public static class MontoFormato
{
    /// <summary>Reformatea lo que el usuario acaba de escribir: agrupa la parte entera con comas de
    /// millar, conserva el punto decimal tal cual se va escribiendo (sin forzar 2 decimales todavía,
    /// para no bloquear que el usuario siga tecleando).</summary>
    public static string EnVivo(string textoCrudo)
    {
        var soloDigitosYPunto = new string(textoCrudo.Where(c => char.IsDigit(c) || c == '.').ToArray());

        var primerPunto = soloDigitosYPunto.IndexOf('.');
        if (primerPunto >= 0)
        {
            soloDigitosYPunto = soloDigitosYPunto[..(primerPunto + 1)] + soloDigitosYPunto[(primerPunto + 1)..].Replace(".", "");
        }

        var partes = soloDigitosYPunto.Split('.');
        if (partes.Length == 2 && partes[1].Length > 2)
        {
            partes[1] = partes[1][..2];
        }

        var parteEntera = partes[0].Length == 0
            ? ""
            : decimal.Parse(partes[0], CultureInfo.InvariantCulture).ToString("N0", CultureInfo.InvariantCulture);

        return partes.Length == 2 ? $"{parteEntera}.{partes[1]}" : parteEntera;
    }

    public static decimal? AValor(string textoFormateado)
    {
        var limpio = textoFormateado.Replace(",", "");
        return decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor) ? valor : null;
    }
}
