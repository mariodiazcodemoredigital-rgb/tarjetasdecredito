using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TarjetasCredito.Domain.Buro;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Infrastructure.Buro;
using TarjetasCredito.Infrastructure.Data;
using TarjetasCredito.Infrastructure.Email;
using TarjetasCredito.Infrastructure.Identity;
using TarjetasCredito.Infrastructure.Push;
using TarjetasCredito.Infrastructure.Reminders;
using TarjetasCredito.Infrastructure.Repositories;
using TarjetasCredito.Server;

var builder = WebApplication.CreateBuilder(args);

const string ClientCorsPolicy = "ClientCorsPolicy";
var clientOrigins = builder.Configuration.GetSection("ClientOrigins").Get<string[]>()
    ?? ["https://localhost:7160", "http://localhost:5138"];

builder.Services.AddControllers();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=tarjetascredito.db"));

// Envío real de correo (confirmación/recuperación de contraseña) — ver SPEC-004 "Envío de correo".
// Se registra antes de AddIdentityApiEndpoints para que Identity lo resuelva en vez de su envío no-op.
builder.Services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();

// Identity con endpoints de API (bearer token) — sin Duende IdentityServer. Ver SPEC-001 y SPEC-004.
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedEmail = builder.Environment.IsProduction();
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<DescriptorErroresIdentityEspanol>();

// FaceID/WebAuthn (passkeys nativos de Identity en .NET 10) — ver SPEC-004. El RP ID debe
// coincidir con el dominio del Client; en local "localhost" sirve para ambos puertos.
builder.Services.Configure<IdentityPasskeyOptions>(options =>
    options.ServerDomain = builder.Configuration["WebAuthn:ServerDomain"] ?? "localhost");

// SignInManager guarda el reto de la ceremonia WebAuthn (entre "opciones" y "registro"/"acceso")
// en una cookie efímera (reutiliza el esquema de cookie de 2FA de Identity). Client y Server viven
// en orígenes distintos (puertos distintos), así que esa cookie necesita SameSite=None + Secure para
// poder viajar entre ellos — si no, el segundo request nunca la recibe y truena con
// "No passkey attestation is underway" (bug real, 2026-09-09). Ver SPEC-004.
builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy =>
        policy.WithOrigins(clientOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Persistencia de las llaves de Data Protection (cifran el bearer token opaco de Identity, ver
// SPEC-004) fuera del contenedor: sin esto, cada recreación del contenedor Docker invalida todas las
// sesiones activas. Solo se activa si DataProtection:KeysPath está configurado (variable de entorno
// en producción, ver docker-compose.yml) — en desarrollo no cambia nada, Identity sigue usando su
// llavero efímero de siempre.
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("TarjetasCredito");
}

builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPaymentReminderRepository, PaymentReminderRepository>();
builder.Services.AddScoped<ICardPaymentRepository, CardPaymentRepository>();
builder.Services.AddScoped<IBuroCreditoService, BuroCreditoMockService>();
builder.Services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
builder.Services.AddScoped<PaymentReminderGenerationService>();
builder.Services.AddScoped<IWebPushSenderService, WebPushSenderService>();

// Notificaciones Web Push (Sprint 8): primer BackgroundService de la app — ver SPEC-001 y SPEC-003.
builder.Services.AddHostedService<PaymentReminderPushHostedService>();

var app = builder.Build();

// Aplica migraciones pendientes al arrancar — necesario para que el contenedor Docker cree/actualice
// la base SQLite solo, sin tener que entrar a ejecutar `dotnet ef` a mano. Idempotente: no hace nada
// si ya está al día (ver SPEC-001 "Despliegue con Docker").
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TarjetasCredito API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors(ClientCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapIdentityApi<ApplicationUser>();
app.MapPasskeyEndpoints();

app.MapControllers();

// ---------- PWA (Blazor WebAssembly, ver SPEC-001 "Despliegue con Docker") ----------
// Solo tiene efecto en el build publicado (dotnet publish copia el wwwroot del Client aquí) — en
// desarrollo el Client sigue sirviéndose por separado con su propio `dotnet run`, esto no interfiere.
// MapStaticAssets, no UseBlazorFrameworkFiles: en .NET 10 esa rama del pipeline no conecta con estos
// endpoints y produce un 500 (mismo hallazgo real que en NoteReminder). MapFallbackToFile va al final
// a propósito — solo debe capturar rutas que ningún controller/endpoint de arriba ya haya resuelto.
app.MapStaticAssets();
app.MapFallbackToFile("index.html");

app.Run();
