using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Asiento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column("transaccion_id")]
        public Guid TransaccionId { get; set; }

        [Required]
        [Column("cuenta_id")]
        public Guid CuentaId { get; set; }

        [Required]
        [Column("debito", TypeName = "decimal(18, 4)")]
        public decimal Debito { get; set; } = 0;

        [Required]
        [Column("credito", TypeName = "decimal(18, 4)")]
        public decimal Credito { get; set; } = 0;

        [Required]
        [StringLength(3)]
        public string Moneda { get; set; } = "DOP";

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("contabilizado_en")]
        public DateTime ContabilizadoEn { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual Transaccion? Transaccion { get; set; }
        public virtual Cuenta? Cuenta { get; set; }
    }
}