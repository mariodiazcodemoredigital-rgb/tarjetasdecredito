namespace TarjetasCredito.Domain.Reglas;

/// <summary>Paleta fija para autoasignar color a una tarjeta cuando el usuario no elige uno (ver SPEC-005).</summary>
public static class PaletaColoresTarjeta
{
    private static readonly string[] Colores =
    [
        "#3B5BFE", // azul primario
        "#7C5CFF", // violeta
        "#1FAE6B", // verde
        "#E0562B", // naranja
        "#0EA5C4", // cian
        "#D6489A", // magenta
        "#F2A93B", // ámbar
        "#5C6BC0"  // índigo
    ];

    public static string SiguienteColor(int cantidadTarjetasExistentes)
        => Colores[cantidadTarjetasExistentes % Colores.Length];
}
