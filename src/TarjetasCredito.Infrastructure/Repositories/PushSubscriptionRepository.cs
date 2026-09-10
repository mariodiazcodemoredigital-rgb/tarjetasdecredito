using Microsoft.EntityFrameworkCore;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Data;

namespace TarjetasCredito.Infrastructure.Repositories;

public class PushSubscriptionRepository(AppDbContext db) : IPushSubscriptionRepository
{
    public async Task<IReadOnlyList<PushSubscriptionRecord>> ObtenerPorUsuarioAsync(string userId, CancellationToken ct = default)
        => await db.PushSubscriptions.Where(p => p.UserId == userId).ToListAsync(ct);

    public async Task<IReadOnlyList<PushSubscriptionRecord>> ObtenerPorUsuariosAsync(IReadOnlyCollection<string> userIds, CancellationToken ct = default)
        => await db.PushSubscriptions.Where(p => userIds.Contains(p.UserId)).ToListAsync(ct);

    public async Task CrearOActualizarAsync(PushSubscriptionRecord suscripcion, CancellationToken ct = default)
    {
        var existente = await db.PushSubscriptions.FirstOrDefaultAsync(p => p.Endpoint == suscripcion.Endpoint, ct);
        if (existente is null)
        {
            db.PushSubscriptions.Add(suscripcion);
        }
        else
        {
            existente.UserId = suscripcion.UserId;
            existente.P256dh = suscripcion.P256dh;
            existente.Auth = suscripcion.Auth;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(string userId, string endpoint, CancellationToken ct = default)
    {
        var existente = await db.PushSubscriptions.FirstOrDefaultAsync(p => p.UserId == userId && p.Endpoint == endpoint, ct);
        if (existente is null)
        {
            return;
        }

        db.PushSubscriptions.Remove(existente);
        await db.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var existente = await db.PushSubscriptions.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existente is null)
        {
            return;
        }

        db.PushSubscriptions.Remove(existente);
        await db.SaveChangesAsync(ct);
    }
}
