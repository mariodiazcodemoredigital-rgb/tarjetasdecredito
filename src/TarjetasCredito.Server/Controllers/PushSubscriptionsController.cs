using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Push;
using TarjetasCredito.Shared.Dtos;
using WebPush;

namespace TarjetasCredito.Server.Controllers;

/// <summary>
/// Suscripciones Web Push (Sprint 8, ver SPEC-004 "Notificaciones push"). A diferencia de los
/// endpoints de passkey, este es un Controller normal — no hay ceremonia cross-origin de por medio.
/// </summary>
[Route("api/[controller]")]
public class PushSubscriptionsController(
    IPushSubscriptionRepository suscripciones,
    IConfiguration configuration,
    IWebPushSenderService sender) : ApiControllerBase
{
    [HttpGet("vapid-public-key")]
    public ActionResult<VapidClavePublicaDto> ObtenerClavePublica()
    {
        var clavePublica = configuration["WebPush:VapidPublicKey"];
        if (string.IsNullOrWhiteSpace(clavePublica))
        {
            return NotFound();
        }

        return Ok(new VapidClavePublicaDto(clavePublica));
    }

    [HttpPost]
    public async Task<IActionResult> Suscribir([FromBody] SuscripcionPushRequest request, CancellationToken ct)
    {
        await suscripciones.CrearOActualizarAsync(new PushSubscriptionRecord
        {
            UserId = UserId,
            Endpoint = request.Endpoint,
            P256dh = request.P256dh,
            Auth = request.Auth
        }, ct);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Desuscribir([FromQuery] string endpoint, CancellationToken ct)
    {
        await suscripciones.EliminarAsync(UserId, endpoint, ct);
        return Ok();
    }

    /// <summary>
    /// Envía un push de prueba de inmediato a todas las suscripciones del usuario, sin pasar por el
    /// PaymentReminderPushHostedService — para que el usuario pueda confirmar que sus notificaciones
    /// llegan sin tener que esperar una fecha real de recordatorio ni un reinicio del servidor.
    /// </summary>
    [HttpPost("prueba")]
    public async Task<IActionResult> EnviarPrueba(CancellationToken ct)
    {
        var subs = await suscripciones.ObtenerPorUsuarioAsync(UserId, ct);
        if (subs.Count == 0)
        {
            return BadRequest("No tienes ninguna suscripción activa. Activa las notificaciones primero.");
        }

        var payload = JsonSerializer.Serialize(new
        {
            title = "Notificación de prueba",
            body = "Si ves esto, tus notificaciones push están funcionando correctamente.",
            url = "/pagos"
        });

        var errores = new List<string>();
        foreach (var sub in subs)
        {
            try
            {
                await sender.EnviarAsync(sub, payload, ct);
            }
            catch (WebPushException ex)
            {
                if (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                {
                    await suscripciones.EliminarAsync(sub.Id, ct);
                }

                errores.Add($"{(int)ex.StatusCode} {ex.StatusCode}: {ex.Message}");
            }
        }

        if (errores.Count > 0)
        {
            return BadRequest("No se pudo enviar la notificación de prueba: " + string.Join(" | ", errores));
        }

        return Ok();
    }
}
