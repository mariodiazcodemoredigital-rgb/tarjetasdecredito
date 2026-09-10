namespace TarjetasCredito.Shared.Dtos;

/// <summary>Ver SPEC-004 "FaceID / biometría" y SPEC-001 "Endpoints de passkeys".</summary>
public record PasskeyCredentialRequest(string CredentialJson);

public record PasskeyInfoDto(string CredentialId, DateTimeOffset CreatedAt);
