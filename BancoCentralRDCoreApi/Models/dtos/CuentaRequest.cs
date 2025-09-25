using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class CuentaRequest
    {
        public Guid? ClienteId { get; set; }
        public string Producto { get; set; }
        public string Moneda { get; set; }
    }
}