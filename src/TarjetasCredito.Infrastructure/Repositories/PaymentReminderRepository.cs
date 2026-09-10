using Microsoft.EntityFrameworkCore;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Data;

namespace TarjetasCredito.Infrastructure.Repositories;

public class PaymentReminderRepository(AppDbContext db) : IPaymentReminderRepository
{
    public async Task<IReadOnlyList<PaymentReminder>> ObtenerPorUsuarioAsync(string userId, CancellationToken ct = default)
        => await db.PaymentReminders
            .Include(r => r.Tarjeta)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.FechaLimitePago)
            .ToListAsync(ct);

    public Task<PaymentReminder?> ObtenerPorIdAsync(string userId, Guid id, CancellationToken ct = default)
        => db.PaymentReminders.Include(r => r.Tarjeta).FirstOrDefaultAsync(r => r.UserId == userId && r.Id == id, ct);

    public Task<PaymentReminder?> ObtenerPorTarjetaYCicloAsync(string userId, Guid creditCardId, DateTime cicloFin, CancellationToken ct = default)
        => db.PaymentReminders.FirstOrDefaultAsync(
            r => r.UserId == userId && r.CreditCardId == creditCardId && r.CicloFin == cicloFin, ct);

    public async Task<PaymentReminder> CrearAsync(PaymentReminder reminder, CancellationToken ct = default)
    {
        db.PaymentReminders.Add(reminder);
        await db.SaveChangesAsync(ct);
        return reminder;
    }

    public Task GuardarCambiosAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
