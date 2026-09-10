using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MimeKit;
using TarjetasCredito.Infrastructure.Identity;

namespace TarjetasCredito.Infrastructure.Email;

/// <summary>
/// Implementación real de <see cref="IEmailSender{TUser}"/> (interfaz nativa de Identity desde .NET 8,
/// usada por MapIdentityApi para confirmación de correo y recuperación de contraseña — ver SPEC-004
/// "Envío de correo"). Usa MailKit contra un buzón SMTP real, no System.Net.Mail.SmtpClient.
/// </summary>
public class SmtpEmailSender(IConfiguration configuration) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        EnviarAsync(email, "Confirma tu correo — TarjetasCredito", $"""
            <p>Hola,</p>
            <p>Confirma tu correo para poder iniciar sesión en TarjetasCredito:</p>
            <p><a href="{confirmationLink}">Confirmar mi correo</a></p>
            <p>Si no creaste esta cuenta, puedes ignorar este mensaje.</p>
            """);

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        EnviarAsync(email, "Recupera tu contraseña — TarjetasCredito", $"""
            <p>Hola,</p>
            <p>Restablece tu contraseña de TarjetasCredito con este enlace:</p>
            <p><a href="{resetLink}">Restablecer contraseña</a></p>
            <p>Si no lo pediste tú, puedes ignorar este mensaje.</p>
            """);

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        EnviarAsync(email, "Código para recuperar tu contraseña — TarjetasCredito", $"""
            <p>Hola,</p>
            <p>Tu código para restablecer tu contraseña en TarjetasCredito es:</p>
            <p style="font-size:1.4rem;font-weight:700;letter-spacing:0.05em;">{resetCode}</p>
            <p>Si no lo pediste tú, puedes ignorar este mensaje.</p>
            """);

    private async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var host = configuration["Smtp:Host"] ?? throw new InvalidOperationException("Falta Smtp:Host en la configuración/user-secrets.");
        var port = int.Parse(configuration["Smtp:Port"] ?? "465");
        var usuario = configuration["Smtp:Username"] ?? throw new InvalidOperationException("Falta Smtp:Username en la configuración/user-secrets.");
        var password = configuration["Smtp:Password"] ?? throw new InvalidOperationException("Falta Smtp:Password en la configuración/user-secrets.");
        var fromDireccion = configuration["Smtp:FromAddress"] ?? usuario;
        var fromNombre = configuration["Smtp:FromName"] ?? "TarjetasCredito";

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(fromNombre, fromDireccion));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;
        mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
        await client.AuthenticateAsync(usuario, password);
        await client.SendAsync(mensaje);
        await client.DisconnectAsync(true);
    }
}
