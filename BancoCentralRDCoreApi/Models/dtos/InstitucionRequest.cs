using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class InstitucionRequest
    {
        public string CodigoSib { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
    }
}