namespace TarjetasCredito.Domain.Entities;

public class BuroScoreSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int Score { get; set; }

    /// <summary>"Mock" hasta que se conecte un proveedor real (ver SPEC-003).</summary>
    public string Proveedor { get; set; } = "Mock";

    /// <summary>Detalle serializado (JSON) de los factores que afectan el score.</summary>
    public string FactoresJson { get; set; } = "[]";
}
