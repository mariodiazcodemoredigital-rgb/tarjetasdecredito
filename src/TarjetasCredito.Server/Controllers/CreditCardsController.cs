using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Domain.Reglas;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server.Controllers;

[Route("api/[controller]")]
public class CreditCardsController(ICreditCardRepository tarjetas, IPurchaseRepository compras, ICardPaymentRepository abonos) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CreditCardDto>>> ObtenerTodas(CancellationToken ct)
    {
        var activas = await tarjetas.ObtenerPorUsuarioAsync(UserId, soloActivas: true, ct);
        var resultado = new List<CreditCardDto>();

        foreach (var t in activas)
        {
            resultado.Add(ToDto(t, await CalcularUtilizacionAsync(t, ct)));
        }

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<CreditCardDto>> Crear([FromBody] CrearCreditCardRequest request, CancellationToken ct)
    {
        var error = ValidarRequest(request);
        if (error is not null)
        {
            return BadRequest(error);
        }

        var colorHex = string.IsNullOrWhiteSpace(request.ColorHex)
            ? PaletaColoresTarjeta.SiguienteColor((await tarjetas.ObtenerPorUsuarioAsync(UserId, soloActivas: false, ct)).Count)
            : request.ColorHex;

        var tarjeta = new CreditCard
        {
            UserId = UserId,
            Nombre = request.Nombre,
            Banco = request.Banco,
            UltimosCuatroDigitos = request.UltimosCuatroDigitos,
            Marca = (MarcaTarjeta)request.Marca,
            LimiteCredito = request.LimiteCredito,
            DiaCorte = request.DiaCorte,
            DiasParaPago = request.DiasParaPago,
            TasaInteresAnual = request.TasaInteresAnual,
            ColorHex = colorHex
        };

        var creada = await tarjetas.CrearAsync(tarjeta, ct);
        return CreatedAtAction(nameof(ObtenerTodas), ToDto(creada, 0m));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CreditCardDto>> Actualizar(Guid id, [FromBody] CrearCreditCardRequest request, CancellationToken ct)
    {
        var existente = await tarjetas.ObtenerPorIdAsync(UserId, id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        var error = ValidarRequest(request);
        if (error is not null)
        {
            return BadRequest(error);
        }

        existente.Nombre = request.Nombre;
        existente.Banco = request.Banco;
        existente.UltimosCuatroDigitos = request.UltimosCuatroDigitos;
        existente.Marca = (MarcaTarjeta)request.Marca;
        existente.LimiteCredito = request.LimiteCredito;
        existente.DiaCorte = request.DiaCorte;
        existente.DiasParaPago = request.DiasParaPago;
        existente.TasaInteresAnual = request.TasaInteresAnual;
        existente.ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? existente.ColorHex : request.ColorHex;

        await tarjetas.ActualizarAsync(existente, ct);
        return Ok(ToDto(existente, await CalcularUtilizacionAsync(existente, ct)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var ok = await tarjetas.DesactivarAsync(UserId, id, ct);
        return ok ? NoContent() : NotFound();
    }

    private static string? ValidarRequest(CrearCreditCardRequest request)
    {
        if (request.DiaCorte is < 1 or > 31)
        {
            return "DiaCorte debe estar entre 1 y 31.";
        }
        if (request.UltimosCuatroDigitos.Length != 4 || !request.UltimosCuatroDigitos.All(char.IsDigit))
        {
            return "UltimosCuatroDigitos debe tener exactamente 4 dígitos.";
        }
        return null;
    }

    private async Task<decimal> CalcularUtilizacionAsync(CreditCard t, CancellationToken ct)
    {
        var ultimoCorte = CicloFacturacionHelper.UltimoCorte(t.DiaCorte, DateTime.UtcNow);
        var saldoCompras = await compras.ObtenerSaldoCicloVigenteAsync(UserId, t.Id, ultimoCorte, ct);
        var totalAbonado = await abonos.ObtenerSumaCicloVigenteAsync(UserId, t.Id, ultimoCorte, ct);
        var saldoNeto = Math.Max(0m, saldoCompras - totalAbonado);
        return t.LimiteCredito > 0 ? saldoNeto / t.LimiteCredito : 0m;
    }

    private static CreditCardDto ToDto(CreditCard t, decimal utilizacion) => new(
        t.Id, t.Nombre, t.Banco, t.UltimosCuatroDigitos, (MarcaTarjetaDto)t.Marca,
        t.LimiteCredito, t.DiaCorte, t.DiasParaPago, t.TasaInteresAnual, t.Activa, utilizacion, t.ColorHex);
}
