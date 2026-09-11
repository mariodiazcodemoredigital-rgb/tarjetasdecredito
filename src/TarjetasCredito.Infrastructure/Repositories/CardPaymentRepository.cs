using Microsoft.EntityFrameworkCore;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Data;

namespace TarjetasCredito.Infrastructure.Repositories;

public class CardPaymentRepository(AppDbContext db) : ICardPaymentRepository
{
    public async Task<IReadOnlyList<CardPayment>> ObtenerPorUsuarioAsync(string userId, Guid? creditCardId = null, CancellationToken ct = default)
    {
        var query = db.CardPayments.Where(p => p.UserId == userId);
        if (creditCardId.HasValue)
        {
            query = query.Where(p => p.CreditCardId == creditCardId.Value);
        }
        return await query.OrderByDescending(p => p.Fecha).ToListAsync(ct);
    }

    public Task<CardPayment?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default)
        => db.CardPayments.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == id, ct);

    public async Task<CardPayment> CrearAsync(CardPayment abono, CancellationToken ct = default)
    {
        db.CardPayments.Add(abono);
        await db.SaveChangesAsync(ct);
        return abono;
    }

    public async Task ActualizarAsync(CardPayment abono, CancellationToken ct = default)
    {
        var existente = await db.CardPayments.FirstOrDefaultAsync(p => p.UserId == abono.UserId && p.Id == abono.Id, ct);
        if (existente is null)
        {
            return;
        }

        existente.CreditCardId = abono.CreditCardId;
        existente.Monto = abono.Monto;
        existente.Fecha = abono.Fecha;
        existente.Nota = abono.Nota;

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> EliminarAsync(string userId, Guid id, CancellationToken ct = default)
    {
        var existente = await db.CardPayments.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == id, ct);
        if (existente is null)
        {
            return false;
        }

        db.CardPayments.Remove(existente);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<decimal> ObtenerSumaCicloVigenteAsync(string userId, Guid creditCardId, DateTime desde, CancellationToken ct = default)
    {
        var suma = await db.CardPayments
            .Where(p => p.UserId == userId && p.CreditCardId == creditCardId && p.Fecha >= desde)
            .SumAsync(p => (decimal?)p.Monto, ct);
        return suma ?? 0m;
    }
}
