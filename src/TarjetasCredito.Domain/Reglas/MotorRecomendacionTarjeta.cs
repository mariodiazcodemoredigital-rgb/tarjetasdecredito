using TarjetasCredito.Domain.Entities;

namespace TarjetasCredito.Domain.Reglas;

/// <summary>Tarjeta activa junto con su saldo usado estimado (suma de compras del ciclo vigente).</summary>
public record TarjetaEvaluada(CreditCard Tarjeta, decimal SaldoActual);

public record OpcionTarjeta(
    CreditCard Tarjeta,
    int DiasDesdeCorte,
    DateTime ProximoCorte,
    DateTime FechaLimitePago,
    int DiasHastaPago,
    decimal UtilizacionResultante,
    bool ExcedeUtilizacionRecomendada,
    bool CreditoInsuficiente);

public record ResultadoRecomendacion(
    CreditCard? TarjetaRecomendada,
    DateTime? FechaLimitePagoResultante,
    decimal? UtilizacionResultante,
    string Explicacion,
    bool Advertencia,
    IReadOnlyList<OpcionTarjeta> Opciones);

/// <summary>
/// Motor de reglas determinístico que recomienda qué tarjeta usar para una compra hoy,
/// maximizando el plazo sin intereses y evitando que la utilización resultante supere el 30% (ver SPEC-003).
/// </summary>
public static class MotorRecomendacionTarjeta
{
    public const decimal UtilizacionMaximaRecomendada = 0.30m;

    public static ResultadoRecomendacion Recomendar(IReadOnlyList<TarjetaEvaluada> tarjetas, decimal monto, DateTime fecha)
    {
        var opciones = new List<OpcionTarjeta>();

        foreach (var t in tarjetas.Where(t => t.Tarjeta.Activa))
        {
            var ultimoCorte = CicloFacturacionHelper.UltimoCorte(t.Tarjeta.DiaCorte, fecha);
            var proximoCorte = CicloFacturacionHelper.ProximoCorte(t.Tarjeta.DiaCorte, fecha);
            var diasDesdeCorte = (fecha.Date - ultimoCorte.Date).Days;
            var fechaLimitePago = proximoCorte.AddDays(t.Tarjeta.DiasParaPago);
            var diasHastaPago = (fechaLimitePago.Date - fecha.Date).Days;

            var disponible = t.Tarjeta.LimiteCredito - t.SaldoActual;
            var creditoInsuficiente = disponible < monto;
            var utilizacionResultante = t.Tarjeta.LimiteCredito > 0
                ? (t.SaldoActual + monto) / t.Tarjeta.LimiteCredito
                : 1m;

            opciones.Add(new OpcionTarjeta(
                t.Tarjeta, diasDesdeCorte, proximoCorte, fechaLimitePago, diasHastaPago,
                utilizacionResultante, utilizacionResultante > UtilizacionMaximaRecomendada, creditoInsuficiente));
        }

        var elegibles = opciones.Where(o => !o.CreditoInsuficiente).ToList();
        if (elegibles.Count == 0)
        {
            return new ResultadoRecomendacion(null, null, null,
                "Ninguna tarjeta activa tiene crédito disponible suficiente para este monto.", true, opciones);
        }

        var dentroDeLimite = elegibles.Where(o => !o.ExcedeUtilizacionRecomendada).ToList();
        var advertencia = dentroDeLimite.Count == 0;
        var candidatas = dentroDeLimite.Count > 0 ? dentroDeLimite : elegibles;

        var mejor = candidatas
            .OrderByDescending(o => o.DiasHastaPago)
            .ThenBy(o => o.UtilizacionResultante)
            .First();

        var explicacion = advertencia
            ? $"Se recomienda {mejor.Tarjeta.Nombre} porque, aunque toda tu utilización quedaría por encima del 30% recomendado, es la que menor utilización resultante tendría ({mejor.UtilizacionResultante:P0}) y te da hasta el {mejor.FechaLimitePago:dd/MM/yyyy} para pagar."
            : $"Se recomienda {mejor.Tarjeta.Nombre}: tu corte más reciente fue hace {mejor.DiasDesdeCorte} días, lo que te da el mayor plazo — puedes pagar hasta el {mejor.FechaLimitePago:dd/MM/yyyy} ({mejor.DiasHastaPago} días) sin generar intereses, con una utilización resultante de {mejor.UtilizacionResultante:P0}.";

        return new ResultadoRecomendacion(mejor.Tarjeta, mejor.FechaLimitePago, mejor.UtilizacionResultante, explicacion, advertencia, opciones);
    }
}
