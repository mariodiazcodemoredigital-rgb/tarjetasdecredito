namespace TarjetasCredito.Shared.Dtos;

public enum CategoriaCompraDto { Supermercado, Restaurantes, Transporte, Servicios, Entretenimiento, Salud, Otro }

public record PurchaseDto(
    Guid Id,
    Guid CreditCardId,
    string NombreTarjeta,
    decimal Monto,
    string Descripcion,
    CategoriaCompraDto Categoria,
    DateTime Fecha,
    int? Msi);

public record CrearPurchaseRequest(
    Guid CreditCardId,
    decimal Monto,
    string Descripcion,
    CategoriaCompraDto Categoria,
    DateTime Fecha,
    int? Msi);
