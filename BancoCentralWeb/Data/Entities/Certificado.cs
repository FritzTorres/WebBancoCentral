using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Certificado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column("cliente_id")]
        public Guid? ClienteId { get; set; }

        [Column("cuenta_id")]
        public Guid? CuentaId { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [Column("emitido_por_usuario_id")]
        public Guid EmitidoPorUsuarioId { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("emitido_en")]
        public DateTime EmitidoEn { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(10)]
        public string Estado { get; set; } = "emitido";

        [StringLength(200)]
        public string? Hash { get; set; }

        // Propiedades de navegación
        public virtual Cliente? Cliente { get; set; }
        public virtual Cuenta? Cuenta { get; set; }
        public virtual Usuario? EmitidoPor { get; set; }
    }
}