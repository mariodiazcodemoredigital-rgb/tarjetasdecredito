using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TarjetasCredito.Server.Controllers;

/// <summary>
/// El UserId SIEMPRE se obtiene del claim del token autenticado, nunca de un parámetro
/// enviado por el cliente — ver regla de aislamiento en SPEC-004.
/// </summary>
[ApiController]
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Token autenticado sin claim de UserId.");
}
