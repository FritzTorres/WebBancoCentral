using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
    {
        public void Configure(EntityTypeBuilder<Cuenta> builder)
        {
            builder.ToTable("cuentas");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClienteId)
                .IsRequired(false);

            builder.Property(c => c.Producto)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Moneda)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("DOP");

            builder.Property(c => c.Estado)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("activa");

            builder.Property(c => c.Estado)
                .HasAnnotation("CheckConstraint", "estado IN ('activa', 'bloqueada', 'cerrada')");

            builder.Property(c => c.AbiertaEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.Saldo)
                .HasColumnType("decimal(18, 4)")
                .HasDefaultValue(0);

            builder.Property(c => c.InstitucionId)
                .IsRequired(false);

            // Relaciones
            builder.HasOne(c => c.Cliente)
                .WithMany(c => c.Cuentas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Institucion)
                .WithMany(i => i.Cuentas)
                .HasForeignKey(c => c.InstitucionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}