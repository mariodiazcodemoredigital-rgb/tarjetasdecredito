using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain;
using TarjetasCredito.Infrastructure.Identity;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server.Controllers;

[Route("api/[controller]")]
public class ProfileController(UserManager<ApplicationUser> userManager) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> Obtener()
    {
        var user = await userManager.FindByIdAsync(UserId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserProfileDto(user.Email ?? "", user.Nombres, user.Apellidos, (PreferenciaTemaDto)user.ThemePreference));
    }

    [HttpPut]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilRequest request)
    {
        var user = await userManager.FindByIdAsync(UserId);
        if (user is null)
        {
            return NotFound();
        }

        user.Nombres = string.IsNullOrWhiteSpace(request.Nombres) ? null : request.Nombres.Trim();
        user.Apellidos = string.IsNullOrWhiteSpace(request.Apellidos) ? null : request.Apellidos.Trim();
        await userManager.UpdateAsync(user);
        return NoContent();
    }

    [HttpPut("theme")]
    public async Task<IActionResult> ActualizarTema([FromBody] ActualizarTemaRequest request)
    {
        var user = await userManager.FindByIdAsync(UserId);
        if (user is null)
        {
            return NotFound();
        }

        user.ThemePreference = (PreferenciaTema)request.ThemePreference;
        await userManager.UpdateAsync(user);
        return NoContent();
    }
}
