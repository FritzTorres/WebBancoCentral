using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("clientes");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CedulaRnc)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(c => c.CedulaRnc)
                .IsUnique();

            builder.Property(c => c.NombreCompleto)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.TipoCliente)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.TipoCliente)
                .HasAnnotation("CheckConstraint", "tipo_cliente IN ('persona', 'empresa')");

            builder.Property(c => c.KycVigenteHasta)
                .HasColumnType("date");

            builder.Property(c => c.CreadoEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}