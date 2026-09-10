namespace TarjetasCredito.Shared.Dtos;

public enum PreferenciaTemaDto { Light, Dark, System }

public record UserProfileDto(string Email, string? Nombres, string? Apellidos, PreferenciaTemaDto ThemePreference);

public record ActualizarTemaRequest(PreferenciaTemaDto ThemePreference);

public record ActualizarPerfilRequest(string? Nombres, string? Apellidos);
