namespace TarjetasCredito.Domain.Entities;

public class Purchase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public Guid CreditCardId { get; set; }
    public CreditCard? Tarjeta { get; set; }

    public decimal Monto { get; set; }
    public required string Descripcion { get; set; }

    /// <summary>Texto libre (ver SPEC-002 "Categoría de compra: texto libre con sugerencias") — antes
    /// era un enum fijo; el usuario puede escribir una categoría nueva propia.</summary>
    public string Categoria { get; set; } = "Otro";

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    /// <summary>Meses sin intereses, si aplica.</summary>
    public int? Msi { get; set; }
}
