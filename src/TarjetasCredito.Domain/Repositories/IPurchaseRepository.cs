using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Domain.Repositories;

public interface IPurchaseRepository
{
    Task<IReadOnlyList<Purchase>> ObtenerPorUsuarioAsync(string userId, Guid? creditCardId = null, CancellationToken ct = default);
    Task<Purchase?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default);
    Task<Purchase> CrearAsync(Purchase compra, CancellationToken ct = default);
    Task ActualizarAsync(Purchase compra, CancellationToken ct = default);
    Task<bool> EliminarAsync(string userId, Guid id, CancellationToken ct = default);

    /// <summary>Suma de compras del ciclo de facturación vigente (desde el último corte) para una tarjeta.</summary>
    Task<decimal> ObtenerSaldoCicloVigenteAsync(string userId, Guid creditCardId, DateTime desde, CancellationToken ct = default);

    /// <summary>Categorías distintas que el usuario ya ha usado (ver SPEC-002 "Categoría de compra:
    /// texto libre con sugerencias") — para llenar la lista desplegable inteligente.</summary>
    Task<IReadOnlyList<string>> ObtenerCategoriasUsadasAsync(string userId, CancellationToken ct = default);
}
