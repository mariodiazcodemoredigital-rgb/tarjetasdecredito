using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Domain.Repositories;

public interface IPurchaseRepository
{
    Task<IReadOnlyList<Purchase>> ObtenerPorUsuarioAsync(string userId, Guid? creditCardId = null, CancellationToken ct = default);
    Task<Purchase> CrearAsync(Purchase compra, CancellationToken ct = default);

    /// <summary>Suma de compras del ciclo de facturación vigente (desde el último corte) para una tarjeta.</summary>
    Task<decimal> ObtenerSaldoCicloVigenteAsync(string userId, Guid creditCardId, DateTime desde, CancellationToken ct = default);
}
