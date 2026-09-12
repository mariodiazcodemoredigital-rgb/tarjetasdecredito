using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain.Reglas;
using TarjetasCredito.Domain.Repositories;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server.Controllers;

/// <summary>Endpoint del "agente economista": recomienda qué tarjeta usar para una compra (ver SPEC-003).</summary>
[Route("api/[controller]")]
public class RecommendationController(ICreditCardRepository tarjetas, IPurchaseRepository compras) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecomendacionResponse>> Recomendar([FromBody] RecomendacionRequest request, CancellationToken ct)
    {
        if (request.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor a cero.");
        }

        var fecha = (request.Fecha ?? FechaNegocioHelper.AhoraMexico()).Date;
        var activas = await tarjetas.ObtenerPorUsuarioAsync(UserId, soloActivas: true, ct);

        var evaluadas = new List<TarjetaEvaluada>();
        foreach (var t in activas)
        {
            var ultimoCorte = CicloFacturacionHelper.UltimoCorte(t.DiaCorte, fecha);
            var saldo = await compras.ObtenerSaldoCicloVigenteAsync(UserId, t.Id, ultimoCorte, ct);
            evaluadas.Add(new TarjetaEvaluada(t, saldo));
        }

        var resultado = MotorRecomendacionTarjeta.Recomendar(evaluadas, request.Monto, fecha);

        var opciones = resultado.Opciones.Select(o => new OpcionTarjetaDto(
            o.Tarjeta.Id, o.Tarjeta.Nombre, o.DiasDesdeCorte, o.FechaLimitePago, o.DiasHastaPago,
            o.UtilizacionResultante, o.ExcedeUtilizacionRecomendada, o.CreditoInsuficiente)).ToList();

        return Ok(new RecomendacionResponse(
            resultado.TarjetaRecomendada?.Id,
            resultado.TarjetaRecomendada?.Nombre,
            resultado.FechaLimitePagoResultante,
            resultado.UtilizacionResultante,
            resultado.Explicacion,
            resultado.Advertencia,
            opciones));
    }
}
