namespace TarjetasCredito.Domain.Entities;

/// <summary>"Abono" a una tarjeta — ver SPEC-002 y SPEC-003 "Abonos y utilización neta".</summary>
public class CardPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public Guid CreditCardId { get; set; }
    public CreditCard? Tarjeta { get; set; }

    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public string? Nota { get; set; }

    /// <summary>No nulo solo cuando este abono se creó automáticamente al marcar ese recordatorio
    /// como pagado (ver SPEC-003) — null cuando el usuario lo registró manualmente.</summary>
    public Guid? PaymentReminderId { get; set; }
}
