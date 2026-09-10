namespace TarjetasCredito.Client.Services;

/// <summary>
/// Ver SPEC-005 "Idioma": nunca se debe reenviar el cuerpo crudo de una respuesta HTTP fallida a un
/// toast sin verificar qué es. Los `BadRequest("mensaje")` que escriben los controllers de esta app
/// siempre se sirven como texto plano (Content-Type text/plain) — ese es el único caso en el que el
/// cuerpo es, con certeza, un mensaje en español escrito a mano. Cualquier otro Content-Type
/// (ej. application/problem+json del validador automático de [ApiController], o un cuerpo vacío de
/// NotFound()) puede venir en inglés o vacío y no debe mostrarse crudo.
/// </summary>
public static class ErrorMessageHelper
{
    public static async Task<string> ExtraerMensajeAsync(HttpResponseMessage response, string mensajePorDefecto)
    {
        if (response.Content.Headers.ContentType?.MediaType == "text/plain")
        {
            var texto = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }
        }

        return mensajePorDefecto;
    }
}
