namespace TarjetasCredito.Shared.Dtos;

public record RecomendacionRequest(decimal Monto, DateTime? Fecha);

public record OpcionTarjetaDto(
    Guid CreditCardId,
    string NombreTarjeta,
    int DiasDesdeCorte,
    DateTime FechaLimitePago,
    int DiasHastaPago,
    decimal UtilizacionResultante,
    bool ExcedeUtilizacionRecomendada,
    bool CreditoInsuficiente);

public record RecomendacionResponse(
    Guid? CreditCardIdRecomendada,
    string? NombreTarjetaRecomendada,
    DateTime? FechaLimitePagoResultante,
    decimal? UtilizacionResultante,
    string Explicacion,
    bool Advertencia,
    IReadOnlyList<OpcionTarjetaDto> Opciones);
