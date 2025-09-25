using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoCentralWeb.Data.Entities
{
    public class Sesion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column("usuario_id")]
        public Guid UsuarioId { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("emitida_en")]
        public DateTime EmitidaEn { get; set; } = DateTime.UtcNow;

        [Column("expira_en")]
        public DateTime? ExpiraEn { get; set; }

        public bool Activa => ExpiraEn == null || ExpiraEn > DateTime.UtcNow;

        // Propiedades de navegación
        public virtual Usuario? Usuario { get; set; }
    }
}