using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Domain.Repositories;

public interface IPushSubscriptionRepository
{
    Task<IReadOnlyList<PushSubscriptionRecord>> ObtenerPorUsuarioAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<PushSubscriptionRecord>> ObtenerPorUsuariosAsync(IReadOnlyCollection<string> userIds, CancellationToken ct = default);
    Task CrearOActualizarAsync(PushSubscriptionRecord suscripcion, CancellationToken ct = default);
    Task EliminarAsync(string userId, string endpoint, CancellationToken ct = default);
    Task EliminarAsync(Guid id, CancellationToken ct = default);
}
