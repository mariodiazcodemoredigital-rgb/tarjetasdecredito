using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain;
using TarjetasCredito.Domain.Entities;
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

        var resultado = lista.Select(p => new PurchaseDto(
            p.Id, p.CreditCardId, mapaTarjetas.GetValueOrDefault(p.CreditCardId, "(desconocida)"),
            p.Monto, p.Descripcion, (CategoriaCompraDto)p.Categoria, p.Fecha, p.Msi));

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseDto>> Crear([FromBody] CrearPurchaseRequest request, CancellationToken ct)
    {
        var tarjeta = await tarjetas.ObtenerPorIdAsync(UserId, request.CreditCardId, ct);
        if (tarjeta is null)
        {
            return BadRequest("La tarjeta no existe o no pertenece al usuario autenticado.");
        }

        var compra = new Purchase
        {
            UserId = UserId,
            CreditCardId = request.CreditCardId,
            Monto = request.Monto,
            Descripcion = request.Descripcion,
            Categoria = (CategoriaCompra)request.Categoria,
            Fecha = request.Fecha,
            Msi = request.Msi
        };

        var creada = await compras.CrearAsync(compra, ct);
        var dto = new PurchaseDto(creada.Id, creada.CreditCardId, tarjeta.Nombre, creada.Monto,
            creada.Descripcion, (CategoriaCompraDto)creada.Categoria, creada.Fecha, creada.Msi);

        return CreatedAtAction(nameof(ObtenerTodas), dto);
    }
}
