using Microsoft.EntityFrameworkCore;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Data;

namespace TarjetasCredito.Infrastructure.Repositories;

public class PurchaseRepository(AppDbContext db) : IPurchaseRepository
{
    public async Task<IReadOnlyList<Purchase>> ObtenerPorUsuarioAsync(string userId, Guid? creditCardId = null, CancellationToken ct = default)
    {
        var query = db.Purchases.Where(p => p.UserId == userId);
        if (creditCardId.HasValue)
        {
            query = query.Where(p => p.CreditCardId == creditCardId.Value);
        }
        return await query.OrderByDescending(p => p.Fecha).ToListAsync(ct);
    }

    public async Task<Purchase> CrearAsync(Purchase compra, CancellationToken ct = default)
    {
        db.Purchases.Add(compra);
        await db.SaveChangesAsync(ct);
        return compra;
    }

    public async Task<decimal> ObtenerSaldoCicloVigenteAsync(string userId, Guid creditCardId, DateTime desde, CancellationToken ct = default)
    {
        var suma = await db.Purchases
            .Where(p => p.UserId == userId && p.CreditCardId == creditCardId && p.Fecha >= desde)
            .SumAsync(p => (decimal?)p.Monto, ct);
        return suma ?? 0m;
    }
}
