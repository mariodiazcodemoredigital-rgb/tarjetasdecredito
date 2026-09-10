using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Reminders;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server.Controllers;

/// <summary>
/// Recordatorios de pago por ciclo. Generación perezosa bajo demanda (ver SPEC-003) vía
/// PaymentReminderGenerationService — GET asegura que existan los recordatorios del ciclo vigente y
/// el siguiente para cada tarjeta activa antes de devolver la lista. El mismo servicio lo usa
/// PaymentReminderPushHostedService (Sprint 8) para todos los usuarios activos, no solo este.
/// </summary>
[Route("api/[controller]")]
public class PaymentRemindersController(IPaymentReminderRepository recordatorios, PaymentReminderGenerationService generador) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PaymentReminderDto>>> ObtenerTodos(CancellationToken ct)
    {
        await generador.AsegurarRecordatoriosAsync(UserId, ct);

        var ahora = DateTime.UtcNow;
        var todos = await recordatorios.ObtenerPorUsuarioAsync(UserId, ct);
        return Ok(todos.Select(r => ToDto(r, ahora)).ToList());
    }

    [HttpPut("{id:guid}/pagado")]
    public async Task<ActionResult<PaymentReminderDto>> MarcarPagado(Guid id, [FromBody] MarcarPagadoRequest request, CancellationToken ct)
    {
        var recordatorio = await recordatorios.ObtenerPorIdAsync(UserId, id, ct);
        if (recordatorio is null)
        {
            return NotFound();
        }

        recordatorio.Pagado = true;
        recordatorio.FechaPago = request.FechaPago ?? DateTime.UtcNow;
        if (request.MontoEstadoCuenta.HasValue)
        {
            recordatorio.MontoEstadoCuenta = request.MontoEstadoCuenta;
        }

        await recordatorios.GuardarCambiosAsync(ct);
        return Ok(ToDto(recordatorio, DateTime.UtcNow));
    }

    private static PaymentReminderDto ToDto(PaymentReminder r, DateTime ahora)
    {
        var diasHastaLimite = (r.FechaLimitePago.Date - ahora.Date).Days;
        var urgencia = r.Pagado
            ? UrgenciaPagoDto.Normal
            : diasHastaLimite switch
            {
                <= 1 => UrgenciaPagoDto.Vencido, // vencido, vence hoy o mañana
                <= 3 => UrgenciaPagoDto.Inminente,
                <= 7 => UrgenciaPagoDto.Proximo,
                _ => UrgenciaPagoDto.Normal
            };

        return new PaymentReminderDto(
            r.Id, r.CreditCardId, r.Tarjeta?.Nombre ?? "", r.Tarjeta?.ColorHex ?? "#3B5BFE",
            r.CicloInicio, r.CicloFin, r.FechaOptimaPago, r.FechaLimitePago,
            r.MontoEstadoCuenta, r.Pagado, r.FechaPago, diasHastaLimite, urgencia);
    }
}
