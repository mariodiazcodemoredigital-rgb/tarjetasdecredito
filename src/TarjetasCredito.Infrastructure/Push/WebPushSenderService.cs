using Microsoft.Extensions.Configuration;
using TarjetasCredito.Domain.Entities;
using WebPush;

namespace TarjetasCredito.Infrastructure.Push;

public class WebPushSenderService : IWebPushSenderService
{
    private readonly WebPushClient client = new();
    private readonly VapidDetails vapidDetails;

    public WebPushSenderService(IConfiguration configuration)
    {
        var publicKey = configuration["WebPush:VapidPublicKey"]
            ?? throw new InvalidOperationException("Falta WebPush:VapidPublicKey en la configuración/user-secrets.");
        var privateKey = configuration["WebPush:VapidPrivateKey"]
            ?? throw new InvalidOperationException("Falta WebPush:VapidPrivateKey en la configuración/user-secrets.");
        var subject = configuration["WebPush:Subject"]
            ?? throw new InvalidOperationException("Falta WebPush:Subject en la configuración/user-secrets.");

        vapidDetails = new VapidDetails(subject, publicKey, privateKey);
    }

    public Task EnviarAsync(PushSubscriptionRecord suscripcion, string tituloJson, CancellationToken ct = default)
    {
        var subscription = new PushSubscription(suscripcion.Endpoint, suscripcion.P256dh, suscripcion.Auth);
        return client.SendNotificationAsync(subscription, tituloJson, vapidDetails, ct);
    }
}
