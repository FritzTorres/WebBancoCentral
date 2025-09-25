using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using BancoCentralRD.Web.Models.Core;
using BancoCentralRD.Web.Models.DTOs;

namespace BancoCentralRD.Web.Controllers
{
    public class AuthController : ApiController
    {
        private readonly Auth _auth;
        private readonly CommandRouter _router;

        public AuthController()
        {
            _auth = new Auth();
            _router = new CommandRouter();
        }

        [HttpPost]
        [Route("api/auth/login")]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Contrasena))
            {
                return BadRequest("Usuario y contraseña son requeridos");
            }

            var kv = new Dictionary<string, string>
            {
                { "usuario", request.Usuario },
                { "contrasena", request.Contrasena }
            };

            var result = _auth.Login(kv);
            return ProcessResult(result);
        }

        [HttpPost]
        [Route("api/auth/logout")]
        public IHttpActionResult Logout([FromBody] LogoutRequest request)
        {
            if (request == null || request.SessionId == Guid.Empty)
            {
                return BadRequest("SessionId es requerido");
            }

            var result = _auth.Logout(request.SessionId);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/auth/ping")]
        public IHttpActionResult Ping()
        {
            var result = _router.Handle("PING");
            return ProcessResult(result);
        }

        private IHttpActionResult ProcessResult(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return InternalServerError(new Exception("Respuesta vacía del servidor"));
            }

            var parts = result.Split('|');
            if (parts.Length == 0)
            {
                return InternalServerError(new Exception("Formato de respuesta inválido"));
            }

            var status = parts[0];
            if (status == "OK")
            {
                var response = new BaseResponse
                {
                    Success = true,
                    Message = "Operación exitosa",
                    Data = ParseResponseData(parts.Skip(1).ToList())
                };
                return Ok(response);
            }
            else if (status == "ERROR")
            {
                var errorResponse = new BaseResponse
                {
                    Success = false,
                    ErrorCode = GetErrorCode(parts),
                    Message = GetErrorMessage(parts)
                };
                return Content(System.Net.HttpStatusCode.BadRequest, errorResponse);
            }

            return InternalServerError(new Exception("Formato de respuesta desconocido"));
        }

        private object ParseResponseData(List<string> dataParts)
        {
            var data = new Dictionary<string, object>();
            
            foreach (var part in dataParts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0];
                    var value = keyValue[1];

                    // Convertir tipos comunes
                    if (Guid.TryParse(value, out Guid guidValue))
                    {
                        data[key] = guidValue;
                    }
                    else if (DateTime.TryParse(value, out DateTime dateValue))
                    {
                        data[key] = dateValue;
                    }
                    else if (decimal.TryParse(value, out decimal decimalValue))
                    {
                        data[key] = decimalValue;
                    }
                    else if (int.TryParse(value, out int intValue))
                    {
                        data[key] = intValue;
                    }
                    else
                    {
                        data[key] = value;
                    }
                }
            }

            return data;
        }

        private string GetErrorCode(string[] parts)
        {
            foreach (var part in parts)
            {
                if (part.StartsWith("code="))
                {
                    return part.Substring(5);
                }
            }
            return "UNKNOWN_ERROR";
        }

        private string GetErrorMessage(string[] parts)
        {
            foreach (var part in parts)
            {
                if (part.StartsWith("message="))
                {
                    return HttpUtility.UrlDecode(part.Substring(8));
                }
            }
            return "Error desconocido";
        }
    }

    public class LogoutRequest
    {
        public Guid SessionId { get; set; }
    }
}