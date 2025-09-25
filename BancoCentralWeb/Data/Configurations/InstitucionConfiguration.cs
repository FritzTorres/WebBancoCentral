using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Configurations
{
    public class InstitucionConfiguration : IEntityTypeConfiguration<Institucion>
    {
        public void Configure(EntityTypeBuilder<Institucion> builder)
        {
            builder.ToTable("instituciones");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.CodigoSib)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(i => i.CodigoSib)
                .IsUnique();

            builder.Property(i => i.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Tipo)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(i => i.Tipo)
                .HasAnnotation("CheckConstraint", "tipo IN ('banco', 'cooperativa', 'fintech', 'otra')");

            builder.Property(i => i.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(i => i.CreadoEn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}