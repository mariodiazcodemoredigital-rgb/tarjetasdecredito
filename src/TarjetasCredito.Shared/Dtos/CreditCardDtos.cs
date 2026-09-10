namespace TarjetasCredito.Shared.Dtos;

public enum MarcaTarjetaDto { Visa, Mastercard, AmericanExpress, Otra }

public record CreditCardDto(
    Guid Id,
    string Nombre,
    string Banco,
    string UltimosCuatroDigitos,
    MarcaTarjetaDto Marca,
    decimal LimiteCredito,
    int DiaCorte,
    int DiasParaPago,
    decimal? TasaInteresAnual,
    bool Activa,
    decimal UtilizacionActual,
    string ColorHex);

public record CrearCreditCardRequest(
    string Nombre,
    string Banco,
    string UltimosCuatroDigitos,
    MarcaTarjetaDto Marca,
    decimal LimiteCredito,
    int DiaCorte,
    int DiasParaPago,
    decimal? TasaInteresAnual,
    string? ColorHex);
