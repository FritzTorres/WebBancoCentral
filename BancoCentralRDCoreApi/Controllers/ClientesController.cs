using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using BancoCentralRD.Web.Models.Core;
using BancoCentralRD.Web.Models.DTOs;

namespace BancoCentralRD.Web.Controllers
{
    public class ClientesController : ApiController
    {
        private readonly Services _services;
        private readonly Envios _envios;
        private readonly Auth _auth;

        public ClientesController()
        {
            _services = new Services();
            _envios = new Envios();
            _auth = new Auth();
        }

        [HttpPost]
        [Route("api/clientes")]
        public IHttpActionResult CrearCliente([FromBody] ClienteRequest request)
        {
            if (request == null)
            {
                return BadRequest("Datos del cliente son requeridos");
            }

            var sessionId = GetSessionIdFromHeader();
            if (sessionId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (!_auth.IsSessionValid(sessionId))
            {
                return Unauthorized();
            }

            if (!_auth.HasPermission(sessionId, "CREAR_CLIENTE"))
            {
                return Forbidden("No tiene permiso para crear clientes");
            }

            var kv = new Dictionary<string, string>
            {
                { "cedula_rnc", request.CedulaRnc },
                { "nombre_completo", request.NombreCompleto },
                { "tipo_cliente", request.TipoCliente }
            };

            if (request.KycVigenteHasta.HasValue)
            {
                kv["kyc_vigente_hasta"] = request.KycVigenteHasta.Value.ToString("yyyy-MM-dd");
            }

            var result = _services.CrearCliente(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/clientes/{id}")]
        public IHttpActionResult ObtenerCliente(Guid id)
        {
            var sessionId = GetSessionIdFromHeader();
            if (sessionId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (!_auth.IsSessionValid(sessionId))
            {
                return Unauthorized();
            }

            if (!_auth.HasPermission(sessionId, "CONSULTAR_SALDO"))
            {
                return Forbidden("No tiene permiso para consultar clientes");
            }

            var kv = new Dictionary<string, string>
            {
                { "cliente_id", id.ToString() }
            };

            var result = _envios.GetCliente(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/clientes")]
        public IHttpActionResult ListarClientes([FromUri] string q = null, [FromUri] int page = 1, [FromUri] int pageSize = 50)
        {
            var sessionId = GetSessionIdFromHeader();
            if (sessionId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (!_auth.IsSessionValid(sessionId))
            {
                return Unauthorized();
            }

            if (!_auth.HasPermission(sessionId, "CONSULTAR_SALDO"))
            {
                return Forbidden("No tiene permiso para consultar clientes");
            }

            var kv = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "page_size", pageSize.ToString() }
            };

            if (!string.IsNullOrWhiteSpace(q))
            {
                kv["q"] = q;
            }

            var result = _envios.ListClientes(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/clientes/{id}/cuentas")]
        public IHttpActionResult ObtenerResumenCuentas(Guid id)
        {
            var sessionId = GetSessionIdFromHeader();
            if (sessionId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (!_auth.IsSessionValid(sessionId))
            {
                return Unauthorized();
            }

            if (!_auth.HasPermission(sessionId, "CONSULTAR_SALDO"))
            {
                return Forbidden("No tiene permiso para consultar saldos");
            }

            var kv = new Dictionary<string, string>
            {
                { "cliente_id", id.ToString() }
            };

            var result = _envios.GetResumenCuentas(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpPost]
        [Route("api/clientes/{id}/certificados/solvencia")]
        public IHttpActionResult EmitirCertificadoSolvencia(Guid id)
        {
            var sessionId = GetSessionIdFromHeader();
            if (sessionId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (!_auth.IsSessionValid(sessionId))
            {
                return Unauthorized();
            }

            if (!_auth.HasPermission(sessionId, "EMITIR_CERT_SOLVENCIA"))
            {
                return Forbidden("No tiene permiso para emitir certificados de solvencia");
            }

            var kv = new Dictionary<string, string>
            {
                { "cliente_id", id.ToString() }
            };

            var result = _services.EmitirCertSolvencia(sessionId, kv);
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

        private Guid GetSessionIdFromHeader()
        {
            var headers = Request.Headers;
            if (headers.Contains("X-Session-Id"))
            {
                var sessionIdValue = headers.GetValues("X-Session-Id").FirstOrDefault();
                if (Guid.TryParse(sessionIdValue, out Guid sessionId))
                {
                    return sessionId;
                }
            }
            return Guid.Empty;
        }

        private IHttpActionResult Forbidden(string message)
        {
            return Content(System.Net.HttpStatusCode.Forbidden, new BaseResponse
            {
                Success = false,
                ErrorCode = "FORBIDDEN",
                Message = message
            });
        }
    }
}