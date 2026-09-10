namespace TarjetasCredito.Domain.Buro;

public record FactorBuro(string Descripcion, string Impacto);

public record BuroScoreResultado(
    int Score,
    string Proveedor,
    bool EsSimulado,
    IReadOnlyList<FactorBuro> Factores,
    IReadOnlyList<string> Recomendaciones);

/// <summary>
/// Contrato de consulta de buró de crédito. La implementación v1 (BuroCreditoMockService, Sprint 6)
/// es simulada — ver SPEC-003 y SPEC-004. Una implementación real requiere credenciales del usuario
/// para un proveedor concreto (Buró de Crédito México, Círculo de Crédito, Equifax, TransUnion, etc.).
/// </summary>
public interface IBuroCreditoService
{
    Task<BuroScoreResultado> ConsultarScoreAsync(string userId, CancellationToken cancellationToken = default);
}
