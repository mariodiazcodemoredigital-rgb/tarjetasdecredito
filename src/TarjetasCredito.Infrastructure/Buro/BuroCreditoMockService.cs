using TarjetasCredito.Domain.Buro;
using TarjetasCredito.Domain.Reglas;
using TarjetasCredito.Domain.Repositories;

namespace TarjetasCredito.Infrastructure.Buro;

/// <summary>
/// Implementación simulada de IBuroCreditoService (ver SPEC-003). Genera un score y factores
/// derivados de los datos reales que el usuario ya capturó (utilización, tarjetas activas),
/// dejando siempre claro que NO es una consulta real a un buró de crédito.
/// </summary>
public class BuroCreditoMockService(ICreditCardRepository tarjetas, IPurchaseRepository compras) : IBuroCreditoService
{
    public async Task<BuroScoreResultado> ConsultarScoreAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tarjetasUsuario = await tarjetas.ObtenerPorUsuarioAsync(userId, soloActivas: true, cancellationToken);
        var factores = new List<FactorBuro>();
        var recomendaciones = new List<string>();

        var ahora = DateTime.UtcNow;
        decimal limiteTotal = 0m;
        decimal saldoTotal = 0m;
        var tarjetasSobreLimite = 0;

        foreach (var tarjeta in tarjetasUsuario)
        {
            var ultimoCorte = CicloFacturacionHelper.UltimoCorte(tarjeta.DiaCorte, ahora);
            var saldo = await compras.ObtenerSaldoCicloVigenteAsync(userId, tarjeta.Id, ultimoCorte, cancellationToken);
            limiteTotal += tarjeta.LimiteCredito;
            saldoTotal += saldo;

            if (tarjeta.LimiteCredito > 0 && saldo / tarjeta.LimiteCredito > MotorRecomendacionTarjeta.UtilizacionMaximaRecomendada)
            {
                tarjetasSobreLimite++;
            }
        }

        var utilizacionGlobal = limiteTotal > 0 ? saldoTotal / limiteTotal : 0m;

        var score = CalcularScoreSimulado(utilizacionGlobal, tarjetasUsuario.Count);

        if (utilizacionGlobal > MotorRecomendacionTarjeta.UtilizacionMaximaRecomendada)
        {
            factores.Add(new FactorBuro($"Utilización de crédito global en {utilizacionGlobal:P0}, por encima del 30% recomendado", "Negativo"));
            recomendaciones.Add("Paga saldo antes de la fecha de corte para reducir la utilización reportada.");
        }
        else
        {
            factores.Add(new FactorBuro($"Utilización de crédito global saludable ({utilizacionGlobal:P0})", "Positivo"));
        }

        if (tarjetasSobreLimite > 0)
        {
            factores.Add(new FactorBuro($"{tarjetasSobreLimite} tarjeta(s) con utilización individual alta", "Negativo"));
            recomendaciones.Add("Distribuye el consumo entre tus tarjetas o solicita un aumento de línea en la(s) tarjeta(s) más usada(s).");
        }

        if (tarjetasUsuario.Count == 0)
        {
            factores.Add(new FactorBuro("Sin tarjetas activas registradas para evaluar historial", "Neutral"));
            recomendaciones.Add("Registra tus tarjetas y compras para obtener una estimación más precisa.");
        }
        else if (tarjetasUsuario.Count == 1)
        {
            factores.Add(new FactorBuro("Mezcla de crédito limitada (una sola tarjeta activa)", "Neutral"));
        }

        recomendaciones.Add("Nunca dejes pasar la fecha límite de pago: un solo pago tardío es el factor con mayor impacto negativo en el score.");

        return new BuroScoreResultado(
            Score: score,
            Proveedor: "Mock",
            EsSimulado: true,
            Factores: factores,
            Recomendaciones: recomendaciones);
    }

    private static int CalcularScoreSimulado(decimal utilizacionGlobal, int cantidadTarjetas)
    {
        var baseScore = 680;
        var ajustePorUtilizacion = utilizacionGlobal switch
        {
            <= 0.10m => 90,
            <= 0.30m => 40,
            <= 0.50m => -20,
            <= 0.75m => -60,
            _ => -110
        };
        var ajustePorMezcla = cantidadTarjetas switch
        {
            0 => -40,
            1 => -10,
            2 or 3 => 10,
            _ => 20
        };

        var score = baseScore + ajustePorUtilizacion + ajustePorMezcla;
        return Math.Clamp(score, 300, 850);
    }
}
