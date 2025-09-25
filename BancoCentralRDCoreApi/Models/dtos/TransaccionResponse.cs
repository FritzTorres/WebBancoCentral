using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class TransaccionResponse
    {
        public Guid TransaccionId { get; set; }
        public string RefExterna { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public decimal MontoTotal { get; set; }
        public string Moneda { get; set; }
        public DateTime CreadaEn { get; set; }
    }
}