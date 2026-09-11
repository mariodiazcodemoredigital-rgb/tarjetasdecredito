using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TarjetasCredito.Domain.Entities;
using TarjetasCredito.Infrastructure.Identity;

namespace TarjetasCredito.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PaymentReminder> PaymentReminders => Set<PaymentReminder>();
    public DbSet<CardPayment> CardPayments => Set<CardPayment>();
    public DbSet<BuroScoreSnapshot> BuroScoreSnapshots => Set<BuroScoreSnapshot>();
    public DbSet<PushSubscriptionRecord> PushSubscriptions => Set<PushSubscriptionRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CreditCard>(e =>
        {
            e.HasIndex(c => c.UserId);
            e.Property(c => c.UltimosCuatroDigitos).HasMaxLength(4);
            e.HasMany(c => c.Compras)
                .WithOne(p => p.Tarjeta)
                .HasForeignKey(p => p.CreditCardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Purchase>(e =>
        {
            e.HasIndex(p => p.UserId);
            e.HasIndex(p => p.CreditCardId);
        });

        builder.Entity<PaymentReminder>(e =>
        {
            e.HasIndex(p => p.UserId);
            e.HasIndex(p => p.CreditCardId);
            e.HasOne(p => p.Tarjeta)
                .WithMany()
                .HasForeignKey(p => p.CreditCardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CardPayment>(e =>
        {
            e.HasIndex(p => p.UserId);
            e.HasIndex(p => p.CreditCardId);
            e.HasOne(p => p.Tarjeta)
                .WithMany()
                .HasForeignKey(p => p.CreditCardId)
                .OnDelete(DeleteBehavior.Restrict);
            // Trazabilidad opcional (ver SPEC-003 "Abonos y utilización neta"): si se borra el
            // recordatorio de origen, el abono se conserva (solo pierde la referencia), nunca se borra
            // en cascada — es dinero real que el usuario puso, no un dato derivado.
            e.HasOne<PaymentReminder>()
                .WithMany()
                .HasForeignKey(p => p.PaymentReminderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<BuroScoreSnapshot>(e => e.HasIndex(b => b.UserId));
        builder.Entity<PushSubscriptionRecord>(e =>
        {
            e.HasIndex(p => p.UserId);
            // Único por Endpoint: volver a suscribirse desde el mismo navegador debe actualizar la fila
            // existente (upsert), no duplicarla (Sprint 8).
            e.HasIndex(p => p.Endpoint).IsUnique();
        });

        // FaceID/WebAuthn (Sprint 7): IdentityDbContext<ApplicationUser> hereda hasta la sobrecarga
        // completa de 9 parámetros con IdentityUserPasskey<string> como TUserPasskey (confirmado por
        // reflexión), pero esa entidad no queda incluida en el modelo por convención — hay que
        // declararla explícitamente para que EF Core la mapee y genere la migración. Ver SPEC-004.
        // CredentialId es único por diseño de WebAuthn, se usa como llave; Data (IdentityPasskeyData)
        // es un tipo owned que se guarda en columnas propias de la misma tabla.
        builder.Entity<IdentityUserPasskey<string>>(e =>
        {
            e.HasKey(p => p.CredentialId);
            e.HasIndex(p => p.UserId);
            e.OwnsOne(p => p.Data);
        });
    }
}
