namespace TarjetasCredito.Domain.Reglas;

/// <summary>
/// Categorías sugeridas por defecto (ver SPEC-002 "Categoría de compra: texto libre con
/// sugerencias") — punto de partida de la lista desplegable inteligente; el usuario puede escribir
/// cualquier otra y esa queda disponible como sugerencia la próxima vez (ver
/// IPurchaseRepository.ObtenerCategoriasUsadasAsync).
/// </summary>
public static class CategoriasCompraSugeridas
{
    public static readonly IReadOnlyList<string> PorDefecto =
    [
        "Supermercado", "Restaurantes", "Transporte", "Servicios", "Entretenimiento", "Salud", "Otro"
    ];
}
