using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using TarjetasCredito.Infrastructure.Identity;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server;

/// <summary>
/// Endpoints de WebAuthn/passkeys — minimal API, no Controller. Ver SPEC-001 "Endpoints de
/// passkeys: excepción puntual a Controllers" para el porqué (el endpoint de acceso debe emitir
/// el bearer token exactamente como lo hace MapIdentityApi internamente).
/// </summary>
public static class PasskeyEndpoints
{
    public static void MapPasskeyEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/auth/passkey");

        grupo.MapPost("/registro/opciones", async (
            HttpContext http,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(http.User);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var nombre = string.IsNullOrWhiteSpace(user.Nombres) ? user.Email! : user.Nombres;
            var entidad = new PasskeyUserEntity { Id = user.Id, Name = user.Email!, DisplayName = nombre };
            var opciones = await signInManager.MakePasskeyCreationOptionsAsync(entidad);
            return Results.Text(opciones, "application/json");
        }).RequireAuthorization();

        grupo.MapPost("/registro", async (
            PasskeyCredentialRequest request,
            HttpContext http,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(http.User);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var resultado = await signInManager.PerformPasskeyAttestationAsync(request.CredentialJson);
            if (!resultado.Succeeded || resultado.Passkey is null)
            {
                return Results.BadRequest(new { error = "No se pudo registrar la credencial." });
            }

            await userManager.AddOrUpdatePasskeyAsync(user, resultado.Passkey);
            return Results.Ok();
        }).RequireAuthorization();

        grupo.MapPost("/acceso/opciones", async (SignInManager<ApplicationUser> signInManager) =>
        {
            var opciones = await signInManager.MakePasskeyRequestOptionsAsync(user: null);
            return Results.Text(opciones, "application/json");
        }).AllowAnonymous();

        grupo.MapPost("/acceso", async (
            PasskeyCredentialRequest request,
            SignInManager<ApplicationUser> signInManager) =>
        {
            // Mismo mecanismo que usa MapIdentityApi internamente: fijar el esquema Bearer antes de
            // firmar la sesión hace que el handler de Bearer escriba el AccessTokenResponse directo
            // en la respuesta — ver SPEC-001 para el detalle. PasskeySignInAsync hace la verificación
            // (PerformPasskeyAssertionAsync) y el SignInAsync en un solo paso.
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            var resultado = await signInManager.PasskeySignInAsync(request.CredentialJson);
            if (!resultado.Succeeded)
            {
                return Results.Unauthorized();
            }

            return Results.Empty;
        }).AllowAnonymous();

        grupo.MapGet("/", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(http.User);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var credenciales = await userManager.GetPasskeysAsync(user);
            var dto = credenciales
                .Select(c => new PasskeyInfoDto(WebEncoders.Base64UrlEncode(c.CredentialId), c.CreatedAt))
                .ToList();
            return Results.Ok(dto);
        }).RequireAuthorization();

        grupo.MapDelete("/{credentialId}", async (
            string credentialId,
            HttpContext http,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(http.User);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var bytes = WebEncoders.Base64UrlDecode(credentialId);
            await userManager.RemovePasskeyAsync(user, bytes);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
