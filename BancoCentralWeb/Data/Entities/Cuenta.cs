using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Cuenta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column("cliente_id")]
        public Guid? ClienteId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("producto")]
        public string Producto { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        [Column("moneda")]
        public string Moneda { get; set; } = "DOP";

        [Required]
        [StringLength(10)]
        public string Estado { get; set; } = "activa";

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("abierta_en")]
        public DateTime AbiertaEn { get; set; } = DateTime.UtcNow;

        [Column("saldo", TypeName = "decimal(18, 4)")]
        public decimal Saldo { get; set; } = 0;

        [Column("institucion_id")]
        public Guid? InstitucionId { get; set; }

        // Propiedades de navegación
        public virtual Cliente? Cliente { get; set; }
        public virtual Institucion? Institucion { get; set; }
        public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
        public virtual ICollection<Asiento> Asientos { get; set; } = new List<Asiento>();
    }
}