using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class ClienteRequest
    {
        public string CedulaRnc { get; set; }
        public string NombreCompleto { get; set; }
        public string TipoCliente { get; set; }
        public DateTime? KycVigenteHasta { get; set; }
    }
}