using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(30)]
        [Column("cedula_rnc")]
        public string CedulaRnc { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Column("nombre_completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Column("tipo_cliente")]
        public string TipoCliente { get; set; } = string.Empty;

        [Column("kyc_vigente_hasta")]
        public DateTime? KycVigenteHasta { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
        public virtual ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    }
}