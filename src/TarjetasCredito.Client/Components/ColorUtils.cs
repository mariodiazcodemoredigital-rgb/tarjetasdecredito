using System.Globalization;

namespace TarjetasCredito.Client.Components;

public static class ColorUtils
{
    /// <summary>Devuelve el mismo color oscurecido ~28%, usado para el degradado del card visual.</summary>
    public static string Oscurecer(string hex)
    {
        if (!TryParseHex(hex, out var r, out var g, out var b))
        {
            return hex;
        }

        r = (int)(r * 0.72);
        g = (int)(g * 0.72);
        b = (int)(b * 0.72);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static bool TryParseHex(string hex, out int r, out int g, out int b)
    {
        r = g = b = 0;
        var h = hex.TrimStart('#');
        if (h.Length != 6)
        {
            return false;
        }

        return int.TryParse(h[..2], NumberStyles.HexNumber, null, out r)
            && int.TryParse(h[2..4], NumberStyles.HexNumber, null, out g)
            && int.TryParse(h[4..6], NumberStyles.HexNumber, null, out b);
    }
}
