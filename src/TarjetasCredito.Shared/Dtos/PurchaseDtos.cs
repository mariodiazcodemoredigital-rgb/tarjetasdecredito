namespace TarjetasCredito.Shared.Dtos;

public record PurchaseDto(
    Guid Id,
    Guid CreditCardId,
    string NombreTarjeta,
    decimal Monto,
    string Descripcion,
    string Categoria,
    DateTime Fecha,
    int? Msi);

public record CrearPurchaseRequest(
    Guid CreditCardId,
    decimal Monto,
    string Descripcion,
    string Categoria,
    DateTime Fecha,
    int? Msi);
