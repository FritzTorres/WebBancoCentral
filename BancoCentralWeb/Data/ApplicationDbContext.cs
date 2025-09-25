using Microsoft.EntityFrameworkCore;
using BancoCentralWeb.Data.Entities;
using BancoCentralWeb.Data.Configurations;

namespace BancoCentralWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para todas las entidades
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Sesion> Sesiones { get; set; }
        public DbSet<Institucion> Instituciones { get; set; }
        public DbSet<Certificado> Certificados { get; set; }
        public DbSet<Asiento> Asientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas las configuraciones
            modelBuilder.ApplyConfiguration(new ClienteConfiguration());
            modelBuilder.ApplyConfiguration(new CuentaConfiguration());
            modelBuilder.ApplyConfiguration(new TransaccionConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
            modelBuilder.ApplyConfiguration(new SesionConfiguration());
            modelBuilder.ApplyConfiguration(new InstitucionConfiguration());
            modelBuilder.ApplyConfiguration(new CertificadoConfiguration());
            modelBuilder.ApplyConfiguration(new AsientoConfiguration());

            // Configuraciones adicionales
            modelBuilder.Entity<Cliente>()
                .HasMany(c => c.Cuentas)
                .WithOne(c => c.Cliente)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cliente>()
                .HasMany(c => c.Certificados)
                .WithOne(c => c.Cliente)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cuenta>()
                .HasMany(c => c.Transacciones)
                .WithOne(t => t.Cuenta)
                .HasForeignKey(t => t.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cuenta>()
                .HasMany(c => c.Asientos)
                .WithOne(a => a.Cuenta)
                .HasForeignKey(a => a.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Sesiones)
                .WithOne(s => s.Usuario)
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaccion>()
                .HasMany(t => t.Asientos)
                .WithOne(a => a.Transaccion)
                .HasForeignKey(a => a.TransaccionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Institucion>()
                .HasMany(i => i.Cuentas)
                .WithOne(c => c.Institucion)
                .HasForeignKey(c => c.InstitucionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}