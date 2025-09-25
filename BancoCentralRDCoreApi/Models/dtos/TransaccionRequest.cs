using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class TransaccionRequest
    {
        public string RefExterna { get; set; }
        public string Tipo { get; set; }
        public string Moneda { get; set; }
        public string Glosa { get; set; }
        public List<LineaTransaccion> Lineas { get; set; }
    }

    public class LineaTransaccion
    {
        public Guid CuentaId { get; set; }
        public decimal Debito { get; set; }
        public decimal Credito { get; set; }
    }
}