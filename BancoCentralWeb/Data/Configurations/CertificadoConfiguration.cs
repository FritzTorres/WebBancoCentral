using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class CertificadoConfiguration : IEntityTypeConfiguration<Certificado>
    {
        public void Configure(EntityTypeBuilder<Certificado> builder)
        {
            builder.ToTable("certificados");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClienteId)
                .IsRequired(false);

            builder.Property(c => c.CuentaId)
                .IsRequired(false);

            builder.Property(c => c.Tipo)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Tipo)
                .HasAnnotation("CheckConstraint", "tipo IN ('saldo', 'solvencia')");

            builder.Property(c => c.EmitidoPorUsuarioId)
                .IsRequired();

            builder.Property(c => c.EmitidoEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.Estado)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("emitido");

            builder.Property(c => c.Estado)
                .HasAnnotation("CheckConstraint", "estado IN ('emitido', 'revocado')");

            builder.Property(c => c.Hash)
                .HasMaxLength(200);

            // Relaciones
            builder.HasOne(c => c.Cliente)
                .WithMany(c => c.Certificados)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Cuenta)
                .WithMany()
                .HasForeignKey(c => c.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.EmitidoPor)
                .WithMany()
                .HasForeignKey(c => c.EmitidoPorUsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}