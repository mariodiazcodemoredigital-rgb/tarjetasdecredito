namespace TarjetasCredito.Domain.Entities;

public class Purchase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public Guid CreditCardId { get; set; }
    public CreditCard? Tarjeta { get; set; }

    public decimal Monto { get; set; }
    public required string Descripcion { get; set; }
    public CategoriaCompra Categoria { get; set; } = CategoriaCompra.Otro;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    /// <summary>Meses sin intereses, si aplica.</summary>
    public int? Msi { get; set; }
}
