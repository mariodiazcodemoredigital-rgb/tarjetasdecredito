using Microsoft.AspNetCore.Mvc;
using TarjetasCredito.Domain.Buro;
using TarjetasCredito.Shared.Dtos;

namespace TarjetasCredito.Server.Controllers;

[Route("api/[controller]")]
public class BuroController(IBuroCreditoService buro) : ApiControllerBase
{
    [HttpGet("score")]
    public async Task<ActionResult<BuroScoreResponse>> ObtenerScore(CancellationToken ct)
    {
        var resultado = await buro.ConsultarScoreAsync(UserId, ct);
        return Ok(new BuroScoreResponse(
            resultado.Score,
            resultado.Proveedor,
            resultado.EsSimulado,
            resultado.Factores.Select(f => new FactorBuroDto(f.Descripcion, f.Impacto)).ToList(),
            resultado.Recomendaciones));
    }
}
