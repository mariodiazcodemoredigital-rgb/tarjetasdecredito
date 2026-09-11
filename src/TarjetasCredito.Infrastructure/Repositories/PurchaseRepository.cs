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

    public Task<Purchase?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default)
        => db.Purchases.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == id, ct);

    public async Task<Purchase> CrearAsync(Purchase compra, CancellationToken ct = default)
    {
        db.Purchases.Add(compra);
        await db.SaveChangesAsync(ct);
        return compra;
    }

    public async Task ActualizarAsync(Purchase compra, CancellationToken ct = default)
    {
        var existente = await db.Purchases.FirstOrDefaultAsync(p => p.UserId == compra.UserId && p.Id == compra.Id, ct);
        if (existente is null)
        {
            return;
        }

        existente.CreditCardId = compra.CreditCardId;
        existente.Monto = compra.Monto;
        existente.Descripcion = compra.Descripcion;
        existente.Categoria = compra.Categoria;
        existente.Fecha = compra.Fecha;
        existente.Msi = compra.Msi;

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> EliminarAsync(string userId, Guid id, CancellationToken ct = default)
    {
        var existente = await db.Purchases.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == id, ct);
        if (existente is null)
        {
            return false;
        }

        db.Purchases.Remove(existente);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<decimal> ObtenerSaldoCicloVigenteAsync(string userId, Guid creditCardId, DateTime desde, CancellationToken ct = default)
    {
        var suma = await db.Purchases
            .Where(p => p.UserId == userId && p.CreditCardId == creditCardId && p.Fecha >= desde)
            .SumAsync(p => (decimal?)p.Monto, ct);
        return suma ?? 0m;
    }

    public async Task<IReadOnlyList<string>> ObtenerCategoriasUsadasAsync(string userId, CancellationToken ct = default)
        => await db.Purchases.Where(p => p.UserId == userId).Select(p => p.Categoria).Distinct().ToListAsync(ct);
}
