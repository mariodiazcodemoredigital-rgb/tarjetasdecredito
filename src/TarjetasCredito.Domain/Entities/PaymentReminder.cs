namespace TarjetasCredito.Domain.Entities;

public class PaymentReminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public Guid CreditCardId { get; set; }
    public CreditCard? Tarjeta { get; set; }

    public DateTime CicloInicio { get; set; }
    public DateTime CicloFin { get; set; }
    public DateTime FechaLimitePago { get; set; }

    public decimal? MontoEstadoCuenta { get; set; }
    public bool Pagado { get; set; }
    public DateTime? FechaPago { get; set; }

    /// <summary>Fecha límite - 3 días, recomendada para pagar antes del corte y reducir utilización reportada.</summary>
    public DateTime FechaOptimaPago => CicloFin.AddDays(-3);

    // Avisos push enviados (Sprint 8, ver SPEC-004 "Notificaciones push"): null = no enviado todavía.
    // Marcan idempotencia para que PaymentReminderPushHostedService no reenvíe en cada tick.
    public DateTime? NotificacionOptimaEnviadaUtc { get; set; }
    public DateTime? NotificacionT3EnviadaUtc { get; set; }
    public DateTime? NotificacionT1EnviadaUtc { get; set; }
}
