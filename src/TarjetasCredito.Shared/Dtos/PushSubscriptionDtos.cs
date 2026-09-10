namespace TarjetasCredito.Shared.Dtos;

public record SuscripcionPushRequest(string Endpoint, string P256dh, string Auth);

public record VapidClavePublicaDto(string ClavePublica);
