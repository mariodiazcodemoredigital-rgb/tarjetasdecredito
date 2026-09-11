namespace TarjetasCredito.Shared.Dtos;

public record CardPaymentDto(
    Guid Id,
    Guid CreditCardId,
    string NombreTarjeta,
    string ColorHex,
    decimal Monto,
    DateTime Fecha,
    string? Nota);

public record CrearCardPaymentRequest(Guid CreditCardId, decimal Monto, DateTime Fecha, string? Nota);
