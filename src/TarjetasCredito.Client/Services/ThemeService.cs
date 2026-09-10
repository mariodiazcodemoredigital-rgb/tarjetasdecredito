using System.Net.Http.Json;
using Microsoft.JSInterop;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Client.Services;

public class ThemeService(HttpClient http, IJSRuntime js)
{
    private const string LocalStorageKey = "tarjetascredito.theme";

    public PreferenciaTemaDto Actual { get; private set; } = PreferenciaTemaDto.Light;

    public event Action? CambioTema;

    public async Task InicializarAsync()
    {
        var guardado = await js.InvokeAsync<string?>("localStorage.getItem", LocalStorageKey);
        if (Enum.TryParse<PreferenciaTemaDto>(guardado, out var tema))
        {
            Actual = tema;
            await AplicarAlDomAsync();
        }
    }

    public async Task CargarDesdePerfilAsync()
    {
        try
        {
            var perfil = await http.GetFromJsonAsync<UserProfileDto>("api/profile", JsonDefaults.Options);
            if (perfil is not null)
            {
                await EstablecerAsync(perfil.ThemePreference, sincronizarServidor: false);
            }
        }
        catch
        {
            // sin sesión o sin conexión: se mantiene el tema local
        }
    }

    public async Task EstablecerAsync(PreferenciaTemaDto tema, bool sincronizarServidor = true)
    {
        Actual = tema;
        await js.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, tema.ToString());
        await AplicarAlDomAsync();
        CambioTema?.Invoke();

        if (sincronizarServidor)
        {
            try
            {
                await http.PutAsJsonAsync("api/profile/theme", new ActualizarTemaRequest(tema), JsonDefaults.Options);
            }
            catch
            {
                // se aplicó localmente; se reintentará sincronizar en la próxima carga de perfil
            }
        }
    }

    private Task AplicarAlDomAsync()
    {
        var valor = Actual switch
        {
            PreferenciaTemaDto.Dark => "dark",
            PreferenciaTemaDto.Light => "light",
            _ => "system"
        };
        return js.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", valor).AsTask();
    }
}
