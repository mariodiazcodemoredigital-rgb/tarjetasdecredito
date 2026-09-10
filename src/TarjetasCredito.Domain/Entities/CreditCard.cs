namespace TarjetasCredito.Domain.Entities;

public class CreditCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }

    public required string Nombre { get; set; }
    public required string Banco { get; set; }
    public required string UltimosCuatroDigitos { get; set; }
    public MarcaTarjeta Marca { get; set; } = MarcaTarjeta.Otra;

    public decimal LimiteCredito { get; set; }

    /// <summary>Día del mes (1-31) en que cierra el ciclo de facturación.</summary>
    public int DiaCorte { get; set; }

    /// <summary>Días entre la fecha de corte y la fecha límite de pago del ciclo.</summary>
    public int DiasParaPago { get; set; } = 20;

    public decimal? TasaInteresAnual { get; set; }

    /// <summary>Color del card visual en la UI (ej. "#3B5BFE"). Se autoasigna si el usuario no elige uno (ver SPEC-005).</summary>
    public required string ColorHex { get; set; }

    public bool Activa { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public ICollection<Purchase> Compras { get; set; } = new List<Purchase>();
}
