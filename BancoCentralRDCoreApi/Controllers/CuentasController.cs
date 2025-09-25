using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using BancoCentralRD.Web.Models.Core;
using BancoCentralRD.Web.Models.DTOs;

namespace BancoCentralRD.Web.Controllers
{
    public class CuentasController : ApiController
    {
        private readonly Services _services;
        private readonly Envios _envios;
        private readonly Auth _auth;

        public CuentasController()
        {
            _services = new Services();
            _envios = new Envios();
            _auth = new Auth();
        }

        [HttpPost]
        [Route("api/cuentas")]
        public IHttpActionResult AbrirCuenta([FromBody] CuentaRequest request)
        {
            if (request == null)
            {
                return BadRequest("Datos de la cuenta son requeridos");
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

            if (!_auth.HasPermission(sessionId, "ABRIR_CUENTA"))
            {
                return Forbidden("No tiene permiso para abrir cuentas");
            }

            var kv = new Dictionary<string, string>
            {
                { "producto", request.Producto },
                { "moneda", request.Moneda ?? "DOP" }
            };

            if (request.ClienteId.HasValue)
            {
                kv["cliente_id"] = request.ClienteId.Value.ToString();
            }

            var result = _services.AbrirCuenta(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/cuentas/{id}")]
        public IHttpActionResult ObtenerCuenta(Guid id)
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
                return Forbidden("No tiene permiso para consultar cuentas");
            }

            var kv = new Dictionary<string, string>
            {
                { "cuenta_id", id.ToString() }
            };

            var result = _envios.GetCuenta(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/cuentas/{id}/saldo")]
        public IHttpActionResult ObtenerSaldo(Guid id)
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
                { "cuenta_id", id.ToString() }
            };

            var result = _envios.GetSaldo(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/cuentas/{id}/movimientos")]
        public IHttpActionResult ObtenerMovimientos(Guid id, [FromUri] DateTime? desde = null, [FromUri] DateTime? hasta = null, [FromUri] int page = 1, [FromUri] int pageSize = 50)
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

            if (!_auth.HasPermission(sessionId, "CONSULTAR_MOVIMIENTOS"))
            {
                return Forbidden("No tiene permiso para consultar movimientos");
            }

            var kv = new Dictionary<string, string>
            {
                { "cuenta_id", id.ToString() },
                { "page", page.ToString() },
                { "page_size", pageSize.ToString() }
            };

            if (desde.HasValue)
            {
                kv["desde"] = desde.Value.ToString("yyyy-MM-dd");
            }

            if (hasta.HasValue)
            {
                kv["hasta"] = hasta.Value.ToString("yyyy-MM-dd");
            }

            var result = _envios.GetMovimientos(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/cuentas/{id}/saldo-corte")]
        public IHttpActionResult ObtenerSaldoCorte(Guid id, [FromUri] DateTime fecha)
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
                { "cuenta_id", id.ToString() },
                { "fecha", fecha.ToString("yyyy-MM-dd") }
            };

            var result = _envios.GetSaldoCorte(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpPost]
        [Route("api/cuentas/{id}/certificados/saldo")]
        public IHttpActionResult EmitirCertificadoSaldo(Guid id)
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

            if (!_auth.HasPermission(sessionId, "EMITIR_CERT_SALDO"))
            {
                return Forbidden("No tiene permiso para emitir certificados de saldo");
            }

            var kv = new Dictionary<string, string>
            {
                { "cuenta_id", id.ToString() }
            };

            var result = _services.EmitirCertSaldo(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/cuentas")]
        public IHttpActionResult ListarCuentas([FromUri] Guid? clienteId = null, [FromUri] string moneda = null, [FromUri] string producto = null, [FromUri] int page = 1, [FromUri] int pageSize = 50)
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
                return Forbidden("No tiene permiso para consultar cuentas");
            }

            var kv = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "page_size", pageSize.ToString() }
            };

            if (clienteId.HasValue)
            {
                kv["cliente_id"] = clienteId.Value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(moneda))
            {
                kv["moneda"] = moneda;
            }

            if (!string.IsNullOrWhiteSpace(producto))
            {
                kv["producto"] = producto;
            }

            var result = _envios.ListCuentas(sessionId, kv);
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