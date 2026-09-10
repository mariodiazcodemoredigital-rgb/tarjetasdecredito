using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TarjetasCredito.Client;
using TarjetasCredito.Client.Auth;
using TarjetasCredito.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// En dev (dos procesos separados) viene de wwwroot/appsettings.Development.json. En producción con
// Docker (Server hospeda el Client compilado, mismo origen — ver SPEC-001 "Despliegue con Docker"),
// no hay override y se usa el propio origen del Client, que ya es el del Server.
var serverBaseAddress = builder.Configuration["ServerBaseAddress"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(serverBaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthenticationStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<UserProfileService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<CreditCardApiService>();
builder.Services.AddScoped<PurchaseApiService>();
builder.Services.AddScoped<RecommendationApiService>();
builder.Services.AddScoped<BuroApiService>();
builder.Services.AddScoped<PaymentReminderApiService>();
builder.Services.AddScoped<PasskeyApiService>();
builder.Services.AddScoped<PushNotificationApiService>();

var host = builder.Build();

var authProvider = host.Services.GetRequiredService<TokenAuthenticationStateProvider>();
var sesionRestaurada = await authProvider.IntentarRestaurarSesionAsync();

var themeService = host.Services.GetRequiredService<ThemeService>();
await themeService.InicializarAsync();

if (sesionRestaurada)
{
    var userProfileService = host.Services.GetRequiredService<UserProfileService>();
    await userProfileService.CargarAsync();
    await themeService.CargarDesdePerfilAsync();
}

await host.RunAsync();
