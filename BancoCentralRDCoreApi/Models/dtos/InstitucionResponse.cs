using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class InstitucionResponse
    {
        public Guid InstitucionId { get; set; }
        public string CodigoSib { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}