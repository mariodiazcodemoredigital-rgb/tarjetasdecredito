using Microsoft.AspNetCore.Identity;
using TarjetasCredito.Domain;

namespace TarjetasCredito.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public PreferenciaTema ThemePreference { get; set; } = PreferenciaTema.Light;
}
