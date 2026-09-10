using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Push;
using WebPush;

namespace TarjetasCredito.Infrastructure.Reminders;

/// <summary>
/// Primer BackgroundService de la app (Sprint 8, ver SPEC-001). Cada hora: asegura los recordatorios
/// de todos los usuarios con tarjetas activas (no solo el que abre la app) y manda el aviso push que
/// corresponda por cada uno de los 3 disparos posibles (ver SPEC-004 "Notificaciones push"), marcando
/// el flag correspondiente para no reenviar en el siguiente tick.
/// </summary>
public class PaymentReminderPushHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentReminderPushHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);
        do
        {
            try
            {
                await ProcesarAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error procesando notificaciones push de recordatorios de pago.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcesarAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tarjetas = scope.ServiceProvider.GetRequiredService<ICreditCardRepository>();
        var recordatorios = scope.ServiceProvider.GetRequiredService<IPaymentReminderRepository>();
        var generador = scope.ServiceProvider.GetRequiredService<PaymentReminderGenerationService>();
        var suscripciones = scope.ServiceProvider.GetRequiredService<IPushSubscriptionRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<IWebPushSenderService>();

        var userIds = await tarjetas.ObtenerUserIdsConTarjetasActivasAsync(ct);
        var hoy = DateTime.UtcNow.Date;

        foreach (var userId in userIds)
        {
            await generador.AsegurarRecordatoriosAsync(userId, ct);

            var pendientes = (await recordatorios.ObtenerPorUsuarioAsync(userId, ct))
                .Where(r => !r.Pagado)
                .ToList();
            if (pendientes.Count == 0)
            {
                continue;
            }

            List<PushSubscriptionRecord>? subs = null;
            var huboCambios = false;

            foreach (var r in pendientes)
            {
                foreach (var aviso in AvisosDelDia(r, hoy))
                {
                    subs ??= (await suscripciones.ObtenerPorUsuarioAsync(userId, ct)).ToList();

                    foreach (var sub in subs.ToList())
                    {
                        var payload = JsonSerializer.Serialize(new { title = aviso.Titulo, body = aviso.Cuerpo, url = "/pagos" });
                        try
                        {
                            await sender.EnviarAsync(sub, payload, ct);
                            logger.LogInformation("Push enviado a suscripción {Id} ({Titulo}).", sub.Id, aviso.Titulo);
                        }
                        catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                        {
                            logger.LogInformation("Suscripción {Id} caducada ({StatusCode}), se poda.", sub.Id, ex.StatusCode);
                            await suscripciones.EliminarAsync(sub.Id, ct);
                            subs.Remove(sub);
                        }
                    }

                    aviso.MarcarEnviado(r);
                    huboCambios = true;
                }
            }

            if (huboCambios)
            {
                await recordatorios.GuardarCambiosAsync(ct);
            }
        }
    }

    /// <summary>Los avisos que aún no se han mandado y cuya fecha de disparo es hoy (ver SPEC-004).</summary>
    private static IEnumerable<(Action<PaymentReminder> MarcarEnviado, string Titulo, string Cuerpo)> AvisosDelDia(PaymentReminder r, DateTime hoy)
    {
        if (r.NotificacionOptimaEnviadaUtc is null && hoy == r.FechaOptimaPago.Date)
        {
            yield return (
                x => x.NotificacionOptimaEnviadaUtc = DateTime.UtcNow,
                "Fecha óptima de pago",
                "Hoy conviene pagar tu tarjeta para reducir la utilización que se reporta en el próximo corte.");
        }

        if (r.NotificacionT3EnviadaUtc is null && hoy == r.FechaLimitePago.AddDays(-3).Date)
        {
            yield return (
                x => x.NotificacionT3EnviadaUtc = DateTime.UtcNow,
                "Tu fecha límite se acerca",
                "Faltan 3 días para la fecha límite de pago de tu tarjeta.");
        }

        if (r.NotificacionT1EnviadaUtc is null && hoy == r.FechaLimitePago.AddDays(-1).Date)
        {
            yield return (
                x => x.NotificacionT1EnviadaUtc = DateTime.UtcNow,
                "Último día para pagar sin atraso",
                "Mañana vence tu fecha límite de pago. No lo dejes pasar.");
        }
    }
}
