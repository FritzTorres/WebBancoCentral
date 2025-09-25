using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoCentralRD.Web.Models.DTOs
{
    public class LoginResponse
    {
        public Guid SessionId { get; set; }
        public DateTime? ExpiraEn { get; set; }
    }
}