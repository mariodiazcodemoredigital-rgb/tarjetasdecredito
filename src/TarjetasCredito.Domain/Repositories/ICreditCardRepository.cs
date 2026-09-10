using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Domain.Repositories;

/// <summary>
/// Toda implementación DEBE filtrar por userId en cada operación (ver SPEC-004) —
/// nunca confiar en un userId provisto por el cliente fuera de este parámetro explícito.
/// </summary>
public interface ICreditCardRepository
{
    Task<IReadOnlyList<CreditCard>> ObtenerPorUsuarioAsync(string userId, bool soloActivas = true, CancellationToken ct = default);
    Task<CreditCard?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default);
    Task<CreditCard> CrearAsync(CreditCard tarjeta, CancellationToken ct = default);
    Task ActualizarAsync(CreditCard tarjeta, CancellationToken ct = default);
    Task<bool> DesactivarAsync(string userId, Guid id, CancellationToken ct = default);

    /// <summary>Usuarios con al menos una tarjeta activa (cruza todos los usuarios) — usado por
    /// PaymentReminderPushHostedService para saber a quién revisar (Sprint 8).</summary>
    Task<IReadOnlyList<string>> ObtenerUserIdsConTarjetasActivasAsync(CancellationToken ct = default);
}
