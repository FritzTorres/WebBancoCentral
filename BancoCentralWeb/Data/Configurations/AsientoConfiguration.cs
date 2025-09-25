using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class AsientoConfiguration : IEntityTypeConfiguration<Asiento>
    {
        public void Configure(EntityTypeBuilder<Asiento> builder)
        {
            builder.ToTable("asientos");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.TransaccionId)
                .IsRequired();

            builder.Property(a => a.CuentaId)
                .IsRequired();

            builder.Property(a => a.Debito)
                .HasColumnType("decimal(18, 4)")
                .HasDefaultValue(0);

            builder.Property(a => a.Credito)
                .HasColumnType("decimal(18, 4)")
                .HasDefaultValue(0);

            builder.Property(a => a.Moneda)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("DOP");

            builder.Property(a => a.ContabilizadoEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Restricción: un asiento debe ser débito o crédito, no ambos
            builder.ToTable(table => table.HasCheckConstraint("CK_asientos_una_via", 
                "(debito = 0 AND credito > 0) OR (credito = 0 AND debito > 0)"));

            // Índices para mejor rendimiento
            builder.HasIndex(a => a.TransaccionId);
            builder.HasIndex(a => new { a.CuentaId, a.ContabilizadoEn });

            // Relaciones
            builder.HasOne(a => a.Transaccion)
                .WithMany(t => t.Asientos)
                .HasForeignKey(a => a.TransaccionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Cuenta)
                .WithMany(c => c.Asientos)
                .HasForeignKey(a => a.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}