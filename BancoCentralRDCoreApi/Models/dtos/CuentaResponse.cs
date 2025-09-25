using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class CuentaResponse
    {
        public Guid CuentaId { get; set; }
        public Guid? ClienteId { get; set; }
        public string Producto { get; set; }
        public string Moneda { get; set; }
        public string Estado { get; set; }
        public DateTime AbiertaEn { get; set; }
        public decimal Saldo { get; set; }
    }
}