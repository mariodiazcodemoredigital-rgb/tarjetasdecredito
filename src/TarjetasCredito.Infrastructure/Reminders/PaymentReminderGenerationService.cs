using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Reglas;
using TarjetasCredito.Domain.Repositories;

namespace TarjetasCredito.Infrastructure.Reminders;

/// <summary>
/// Genera bajo demanda los <see cref="PaymentReminder"/> que falten para el ciclo vigente y el
/// siguiente de cada tarjeta activa de un usuario (ver SPEC-003 "Generación y gestión de
/// recordatorios de pago"). Extraído de PaymentRemindersController para que también lo use
/// PaymentReminderPushHostedService (Sprint 8), que lo llama para todos los usuarios activos, no
/// solo el que hace la petición GET.
/// </summary>
public class PaymentReminderGenerationService(ICreditCardRepository tarjetas, IPaymentReminderRepository recordatorios)
{
    public async Task AsegurarRecordatoriosAsync(string userId, CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;
        var activas = await tarjetas.ObtenerPorUsuarioAsync(userId, soloActivas: true, ct);

        foreach (var tarjeta in activas)
        {
            foreach (var (cicloInicio, cicloFin) in CicloFacturacionHelper.CiclosARecordar(tarjeta.DiaCorte, ahora))
            {
                var existente = await recordatorios.ObtenerPorTarjetaYCicloAsync(userId, tarjeta.Id, cicloFin, ct);
                if (existente is not null)
                {
                    continue;
                }

                await recordatorios.CrearAsync(new PaymentReminder
                {
                    UserId = userId,
                    CreditCardId = tarjeta.Id,
                    CicloInicio = cicloInicio,
                    CicloFin = cicloFin,
                    FechaLimitePago = cicloFin.AddDays(tarjeta.DiasParaPago)
                }, ct);
            }
        }
    }
}
