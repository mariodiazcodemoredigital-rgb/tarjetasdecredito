using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server.Controllers;

/// <summary>Abonos a tarjeta — ver SPEC-003 "Abonos y utilización neta".</summary>
[Route("api/[controller]")]
public class CardPaymentsController(ICardPaymentRepository abonos, ICreditCardRepository tarjetas) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CardPaymentDto>>> ObtenerTodos([FromQuery] Guid? creditCardId, CancellationToken ct)
    {
        var lista = await abonos.ObtenerPorUsuarioAsync(UserId, creditCardId, ct);
        var mapaTarjetas = (await tarjetas.ObtenerPorUsuarioAsync(UserId, soloActivas: false, ct))
            .ToDictionary(t => t.Id, t => t);

        var resultado = lista.Select(p => ToDto(p, mapaTarjetas.GetValueOrDefault(p.CreditCardId)));
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<CardPaymentDto>> Crear([FromBody] CrearCardPaymentRequest request, CancellationToken ct)
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

        var abono = new CardPayment
        {
            UserId = UserId,
            CreditCardId = request.CreditCardId,
            Monto = request.Monto,
            Fecha = request.Fecha,
            Nota = request.Nota
        };

        var creado = await abonos.CrearAsync(abono, ct);
        return CreatedAtAction(nameof(ObtenerTodos), ToDto(creado, tarjeta));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CardPaymentDto>> Actualizar(Guid id, [FromBody] CrearCardPaymentRequest request, CancellationToken ct)
    {
        var existente = await abonos.ObtenerPorIdAsync(UserId, id, ct);
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
        existente.Fecha = request.Fecha;
        existente.Nota = request.Nota;

        await abonos.ActualizarAsync(existente, ct);
        return Ok(ToDto(existente, tarjeta));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var ok = await abonos.EliminarAsync(UserId, id, ct);
        return ok ? NoContent() : NotFound();
    }

    private static string? ValidarRequest(CrearCardPaymentRequest request)
    {
        if (request.Monto <= 0)
        {
            return "El monto debe ser mayor a cero.";
        }
        return null;
    }

    private static CardPaymentDto ToDto(CardPayment p, CreditCard? tarjeta) => new(
        p.Id, p.CreditCardId, tarjeta?.Nombre ?? "(desconocida)", tarjeta?.ColorHex ?? "#3B5BFE", p.Monto, p.Fecha, p.Nota);
}
