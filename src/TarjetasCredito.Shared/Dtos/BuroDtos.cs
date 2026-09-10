namespace TarjetasCredito.Shared.Dtos;

public record FactorBuroDto(string Descripcion, string Impacto);

public record BuroScoreResponse(
    int Score,
    string Proveedor,
    bool EsSimulado,
    IReadOnlyList<FactorBuroDto> Factores,
    IReadOnlyList<string> Recomendaciones);
