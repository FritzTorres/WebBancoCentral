using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [StringLength(200)]
        [EmailAddress]
        public string? Correo { get; set; }

        [Required]
        [Column("password_hash")]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [Required]
        [Column("salt")]
        public byte[] Salt { get; set; } = Array.Empty<byte>();

        [Required]
        public bool Activo { get; set; } = true;

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<Sesion> Sesiones { get; set; } = new List<Sesion>();
    }
}