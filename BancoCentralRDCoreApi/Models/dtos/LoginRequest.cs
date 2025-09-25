using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
    }
}