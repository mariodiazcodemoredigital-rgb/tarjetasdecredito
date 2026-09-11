using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Reglas;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server.Controllers;

[Route("api/[controller]")]
public class PurchasesController(IPurchaseRepository compras, ICreditCardRepository tarjetas) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PurchaseDto>>> ObtenerTodas([FromQuery] Guid? creditCardId, CancellationToken ct)
    {
        var lista = await compras.ObtenerPorUsuarioAsync(UserId, creditCardId, ct);
        var mapaTarjetas = (await tarjetas.ObtenerPorUsuarioAsync(UserId, soloActivas: false, ct))
            .ToDictionary(t => t.Id, t => t.Nombre);

        var resultado = lista.Select(p => ToDto(p, mapaTarjetas.GetValueOrDefault(p.CreditCardId, "(desconocida)")));
        return Ok(resultado);
    }

    [HttpGet("categorias")]
    public async Task<ActionResult<IReadOnlyList<string>>> ObtenerCategorias(CancellationToken ct)
    {
        var usadas = await compras.ObtenerCategoriasUsadasAsync(UserId, ct);
        var combinadas = CategoriasCompraSugeridas.PorDefecto
            .Concat(usadas)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return Ok(combinadas);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseDto>> Crear([FromBody] CrearPurchaseRequest request, CancellationToken ct)
    {
        var tarjeta = await tarjetas.ObtenerPorIdAsync(UserId, request.CreditCardId, ct);
        if (tarjeta is null)
        {
            return BadRequest("La tarjeta no existe o no pertenece al usuario autenticado.");
        }

        var error = ValidarRequest(request);
        if (error is not null)
        {
            return BadRequest(error);
        }

        var compra = new Purchase
        {
            UserId = UserId,
            CreditCardId = request.CreditCardId,
            Monto = request.Monto,
            Descripcion = request.Descripcion,
            Categoria = request.Categoria,
            Fecha = request.Fecha,
            Msi = request.Msi
        };

        var creada = await compras.CrearAsync(compra, ct);
        return CreatedAtAction(nameof(ObtenerTodas), ToDto(creada, tarjeta.Nombre));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PurchaseDto>> Actualizar(Guid id, [FromBody] CrearPurchaseRequest request, CancellationToken ct)
    {
        var existente = await compras.ObtenerPorIdAsync(UserId, id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        var tarjeta = await tarjetas.ObtenerPorIdAsync(UserId, request.CreditCardId, ct);
        if (tarjeta is null)
        {
            return BadRequest("La tarjeta no existe o no pertenece al usuario autenticado.");
        }

        var error = ValidarRequest(request);
        if (error is not null)
        {
            return BadRequest(error);
        }

        existente.CreditCardId = request.CreditCardId;
        existente.Monto = request.Monto;
        existente.Descripcion = request.Descripcion;
        existente.Categoria = request.Categoria;
        existente.Fecha = request.Fecha;
        existente.Msi = request.Msi;

        await compras.ActualizarAsync(existente, ct);
        return Ok(ToDto(existente, tarjeta.Nombre));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var ok = await compras.EliminarAsync(UserId, id, ct);
        return ok ? NoContent() : NotFound();
    }

    private static string? ValidarRequest(CrearPurchaseRequest request)
    {
        if (request.Monto <= 0)
        {
            return "El monto debe ser mayor a cero.";
        }
        if (string.IsNullOrWhiteSpace(request.Descripcion))
        {
            return "La descripción es obligatoria.";
        }
        if (string.IsNullOrWhiteSpace(request.Categoria))
        {
            return "La categoría es obligatoria.";
        }
        return null;
    }

    private static PurchaseDto ToDto(Purchase p, string nombreTarjeta) => new(
        p.Id, p.CreditCardId, nombreTarjeta, p.Monto, p.Descripcion, p.Categoria, p.Fecha, p.Msi);
}
