using System.Net.Http.Json;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class UserProfileService(HttpClient http)
{
    public UserProfileDto? Perfil { get; private set; }

    public event Action? CambioPerfil;

    /// <summary>Nombre a mostrar en la UI: Nombres si existe, si no el correo (nunca vacío).</summary>
    public string NombreParaMostrar => string.IsNullOrWhiteSpace(Perfil?.Nombres) ? Perfil?.Email ?? "" : Perfil.Nombres;

    public string Iniciales
    {
        get
        {
            if (Perfil is null)
            {
                return "?";
            }
            if (!string.IsNullOrWhiteSpace(Perfil.Nombres))
            {
                var n = Perfil.Nombres.Trim()[..1];
                var a = string.IsNullOrWhiteSpace(Perfil.Apellidos) ? "" : Perfil.Apellidos.Trim()[..1];
                return (n + a).ToUpperInvariant();
            }
            return Perfil.Email.Length > 0 ? Perfil.Email[..1].ToUpperInvariant() : "?";
        }
    }

    public async Task CargarAsync()
    {
        try
        {
            Perfil = await http.GetFromJsonAsync<UserProfileDto>("api/profile", JsonDefaults.Options);
            CambioPerfil?.Invoke();
        }
        catch
        {
            // sin sesión o sin conexión: se mantiene el valor previo (o null)
        }
    }

    public async Task<bool> ActualizarAsync(string? nombres, string? apellidos)
    {
        try
        {
            var response = await http.PutAsJsonAsync("api/profile", new ActualizarPerfilRequest(nombres, apellidos), JsonDefaults.Options);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            await CargarAsync();
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    public void Limpiar()
    {
        Perfil = null;
        CambioPerfil?.Invoke();
    }
}
