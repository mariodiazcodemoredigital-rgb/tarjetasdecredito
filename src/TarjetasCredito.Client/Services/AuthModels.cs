namespace TarjetasCredito.Client.Services;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password);

public record AccessTokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);

public record IdentityErrorEntry(string Code, string Description);
public record IdentityErrorResponse(string Type, string Title, int Status, IReadOnlyDictionary<string, string[]>? Errors);
