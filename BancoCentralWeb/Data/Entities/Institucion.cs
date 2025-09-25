using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Institucion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        [Column("codigo_sib")]
        public string CodigoSib { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        public bool Activo { get; set; } = true;

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    }
}