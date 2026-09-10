using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Infrastructure.Push;

/// <summary>
/// Envía una notificación Web Push (VAPID) a una suscripción concreta. Ver SPEC-004 "Notificaciones
/// push". Si la suscripción está caducada, el envío lanza <see cref="WebPush.WebPushException"/> con
/// StatusCode 404/410 — el llamador decide si podarla de la base (no se reintenta aquí).
/// </summary>
public interface IWebPushSenderService
{
    Task EnviarAsync(PushSubscriptionRecord suscripcion, string tituloJson, CancellationToken ct = default);
}
