using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class TransaccionConfiguration : IEntityTypeConfiguration<Transaccion>
    {
        public void Configure(EntityTypeBuilder<Transaccion> builder)
        {
            builder.ToTable("transacciones");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ReferenciaExterna)
                .HasMaxLength(100);

            builder.HasIndex(t => t.ReferenciaExterna)
                .IsUnique()
                .HasFilter("[ref_externa] IS NOT NULL");

            builder.Property(t => t.Tipo)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(t => t.Estado)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("borrador");

            builder.Property(t => t.Estado)
                .HasAnnotation("CheckConstraint", "estado IN ('borrador', 'validada', 'contabilizada', 'reversada')");

            builder.Property(t => t.CreadaEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(t => t.ValidadaEn)
                .HasColumnType("datetime2");

            builder.Property(t => t.ContabilizadaEn)
                .HasColumnType("datetime2");

            builder.Property(t => t.MontoTotal)
                .HasColumnType("decimal(18, 4)")
                .IsRequired();

            builder.Property(t => t.Moneda)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("DOP");

            builder.Property(t => t.Descripcion)
                .HasMaxLength(500);

            builder.Property(t => t.CuentaId)
                .IsRequired();

            // Relación
            builder.HasOne(t => t.Cuenta)
                .WithMany(c => c.Transacciones)
                .HasForeignKey(t => t.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}