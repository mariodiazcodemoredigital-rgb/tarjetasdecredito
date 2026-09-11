using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Domain.Repositories;

public interface ICardPaymentRepository
{
    Task<IReadOnlyList<CardPayment>> ObtenerPorUsuarioAsync(string userId, Guid? creditCardId = null, CancellationToken ct = default);
    Task<CardPayment?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default);
    Task<CardPayment> CrearAsync(CardPayment abono, CancellationToken ct = default);
    Task ActualizarAsync(CardPayment abono, CancellationToken ct = default);
    Task<bool> EliminarAsync(string userId, Guid id, CancellationToken ct = default);
    Task<decimal> ObtenerSumaCicloVigenteAsync(string userId, Guid creditCardId, DateTime desde, CancellationToken ct = default);
}
