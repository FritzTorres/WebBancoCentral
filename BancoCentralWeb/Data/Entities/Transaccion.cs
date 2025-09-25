using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Transaccion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [StringLength(100)]
        [Column("ref_externa")]
        public string? ReferenciaExterna { get; set; }

        [Required]
        [StringLength(30)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "borrador";

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("creada_en")]
        public DateTime CreadaEn { get; set; } = DateTime.UtcNow;

        [Column("validada_en")]
        public DateTime? ValidadaEn { get; set; }

        [Column("contabilizada_en")]
        public DateTime? ContabilizadaEn { get; set; }

        [Required]
        [Column("monto_total", TypeName = "decimal(18, 4)")]
        public decimal MontoTotal { get; set; }

        [Required]
        [StringLength(3)]
        public string Moneda { get; set; } = "DOP";

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column("cuenta_id")]
        public Guid CuentaId { get; set; }

        // Propiedades de navegación
        public virtual Cuenta? Cuenta { get; set; }
        public virtual ICollection<Asiento> Asientos { get; set; } = new List<Asiento>();
    }
}