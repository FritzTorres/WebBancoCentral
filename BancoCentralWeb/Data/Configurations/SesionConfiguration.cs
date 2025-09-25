using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class SesionConfiguration : IEntityTypeConfiguration<Sesion>
    {
        public void Configure(EntityTypeBuilder<Sesion> builder)
        {
            builder.ToTable("sesiones");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.UsuarioId)
                .IsRequired();

            builder.Property(s => s.EmitidaEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(s => s.ExpiraEn)
                .HasColumnType("datetime2");

            // Relación
            builder.HasOne(s => s.Usuario)
                .WithMany(u => u.Sesiones)
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}