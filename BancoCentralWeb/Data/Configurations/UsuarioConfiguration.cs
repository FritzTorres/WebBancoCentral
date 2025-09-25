using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.NombreUsuario)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.NombreUsuario)
                .IsUnique();

            builder.Property(u => u.Correo)
                .HasMaxLength(200);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.Salt)
                .IsRequired();

            builder.Property(u => u.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.CreadoEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}