using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Domain.Repositories;

public interface IPaymentReminderRepository
{
    Task<IReadOnlyList<PaymentReminder>> ObtenerPorUsuarioAsync(string userId, CancellationToken ct = default);
    Task<PaymentReminder?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default);
    Task<PaymentReminder?> ObtenerPorTarjetaYCicloAsync(string userId, Guid creditCardId, DateTime cicloFin, CancellationToken ct = default);
    Task<PaymentReminder> CrearAsync(PaymentReminder reminder, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
