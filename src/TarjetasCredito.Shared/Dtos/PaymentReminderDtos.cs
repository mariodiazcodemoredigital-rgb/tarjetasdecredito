namespace TarjetasCredito.Shared.Dtos;

/// <summary>Ver SPEC-003 "Generación y gestión de recordatorios de pago" para los umbrales.</summary>
public enum UrgenciaPagoDto { Vencido, Inminente, Proximo, Normal }

public record PaymentReminderDto(
    Guid Id,
    Guid CreditCardId,
    string NombreTarjeta,
    string ColorHex,
    DateTime CicloInicio,
    DateTime CicloFin,
    DateTime FechaOptimaPago,
    DateTime FechaLimitePago,
    decimal? MontoEstadoCuenta,
    bool Pagado,
    DateTime? FechaPago,
    int DiasHastaLimite,
    UrgenciaPagoDto Urgencia);

public record MarcarPagadoRequest(decimal? MontoEstadoCuenta, DateTime? FechaPago);
