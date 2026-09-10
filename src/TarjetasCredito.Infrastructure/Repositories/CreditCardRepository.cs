using Microsoft.EntityFrameworkCore;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Data;

namespace TarjetasCredito.Infrastructure.Repositories;

public class CreditCardRepository(AppDbContext db) : ICreditCardRepository
{
    public async Task<IReadOnlyList<CreditCard>> ObtenerPorUsuarioAsync(string userId, bool soloActivas = true, CancellationToken ct = default)
    {
        var query = db.CreditCards.Where(c => c.UserId == userId);
        if (soloActivas)
        {
            query = query.Where(c => c.Activa);
        }
        return await query.OrderBy(c => c.Nombre).ToListAsync(ct);
    }

    public Task<CreditCard?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default)
        => db.CreditCards.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id, ct);

    public async Task<CreditCard> CrearAsync(CreditCard tarjeta, CancellationToken ct = default)
    {
        db.CreditCards.Add(tarjeta);
        await db.SaveChangesAsync(ct);
        return tarjeta;
    }

    public async Task ActualizarAsync(CreditCard tarjeta, CancellationToken ct = default)
    {
        var existente = await db.CreditCards.FirstOrDefaultAsync(c => c.UserId == tarjeta.UserId && c.Id == tarjeta.Id, ct);
        if (existente is null)
        {
            return;
        }

        existente.Nombre = tarjeta.Nombre;
        existente.Banco = tarjeta.Banco;
        existente.UltimosCuatroDigitos = tarjeta.UltimosCuatroDigitos;
        existente.Marca = tarjeta.Marca;
        existente.LimiteCredito = tarjeta.LimiteCredito;
        existente.DiaCorte = tarjeta.DiaCorte;
        existente.DiasParaPago = tarjeta.DiasParaPago;
        existente.TasaInteresAnual = tarjeta.TasaInteresAnual;
        existente.Activa = tarjeta.Activa;
        existente.ColorHex = tarjeta.ColorHex;

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DesactivarAsync(string userId, Guid id, CancellationToken ct = default)
    {
        var existente = await db.CreditCards.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id, ct);
        if (existente is null)
        {
            return false;
        }

        existente.Activa = false;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<string>> ObtenerUserIdsConTarjetasActivasAsync(CancellationToken ct = default)
        => await db.CreditCards.Where(c => c.Activa).Select(c => c.UserId).Distinct().ToListAsync(ct);
}
