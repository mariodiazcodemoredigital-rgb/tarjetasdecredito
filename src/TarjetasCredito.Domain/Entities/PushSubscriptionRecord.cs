namespace TarjetasCredito.Domain.Entities;

/// <summary>Suscripción Web Push estándar (VAPID). Nombrada "Record" para no chocar con System.Net PushSubscription.</summary>
public class PushSubscriptionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }

    public required string Endpoint { get; set; }
    public required string P256dh { get; set; }
    public required string Auth { get; set; }

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
}
