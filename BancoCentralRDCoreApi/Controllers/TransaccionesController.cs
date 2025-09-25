using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using BancoCentralRD.Web.Models.Core;
using BancoCentralRD.Web.Models.DTOs;

namespace BancoCentralRD.Web.Controllers
{
    public class TransaccionesController : ApiController
    {
        private readonly Services _services;
        private readonly Envios _envios;
        private readonly Auth _auth;

        public TransaccionesController()
        {
            _services = new Services();
            _envios = new Envios();
            _auth = new Auth();
        }

        [HttpPost]
        [Route("api/transacciones")]
        public IHttpActionResult RegistrarTransaccion([FromBody] TransaccionRequest request)
        {
            if (request == null || request.Lineas == null || request.Lineas.Count == 0)
            {
                return BadRequest("Datos de la transacción son requeridos");
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

            if (!_auth.HasPermission(sessionId, "REGISTRAR_TRANSACCION"))
            {
                return Forbidden("No tiene permiso para registrar transacciones");
            }

            var kv = new Dictionary<string, string>
            {
                { "tipo", request.Tipo },
                { "moneda", request.Moneda ?? "DOP" },
                { "n", request.Lineas.Count.ToString() }
            };

            if (!string.IsNullOrWhiteSpace(request.RefExterna))
            {
                kv["ref_externa"] = request.RefExterna;
            }

            if (!string.IsNullOrWhiteSpace(request.Glosa))
            {
                kv["glosa"] = request.Glosa;
            }

            for (int i = 0; i < request.Lineas.Count; i++)
            {
                var linea = request.Lineas[i];
                kv[$"line{i + 1}_cuenta"] = linea.CuentaId.ToString();
                kv[$"line{i + 1}_debito"] = linea.Debito.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                kv[$"line{i + 1}_credito"] = linea.Credito.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            }

            var result = _services.RegistrarTransaccion(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpPost]
        [Route("api/transacciones/{id}/reversar")]
        public IHttpActionResult ReversarTransaccion(Guid id, [FromBody] ReversarTransaccionRequest request)
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

            if (!_auth.HasPermission(sessionId, "REVERSAR_TRANSACCION"))
            {
                return Forbidden("No tiene permiso para reversar transacciones");
            }

            var kv = new Dictionary<string, string>
            {
                { "transaccion_id", id.ToString() }
            };

            if (request != null && !string.IsNullOrWhiteSpace(request.Motivo))
            {
                kv["motivo"] = request.Motivo;
            }

            var result = _services.ReversarTransaccion(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/transacciones/{id}")]
        public IHttpActionResult ObtenerTransaccion(Guid id)
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
                return Forbidden("No tiene permiso para consultar transacciones");
            }

            // Nota: Este endpoint necesitaría un método en Envios para obtener una transacción específica
            // Por ahora, devolvemos un error indicando que no está implementado
            return NotFound();
        }

        [HttpGet]
        [Route("api/transacciones")]
        public IHttpActionResult ListarTransacciones([FromUri] DateTime? desde = null, [FromUri] DateTime? hasta = null, [FromUri] string tipo = null, [FromUri] Guid? clienteId = null, [FromUri] int page = 1, [FromUri] int pageSize = 50)
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
                return Forbidden("No tiene permiso para consultar transacciones");
            }

            var kv = new Dictionary<string, string>
            {
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

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                kv["tipo"] = tipo;
            }

            if (clienteId.HasValue)
            {
                kv["cliente_id"] = clienteId.Value.ToString();
            }

            var result = _envios.ListTransacciones(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpPost]
        [Route("api/transacciones/rtgs")]
        public IHttpActionResult ProcesarRTGS([FromBody] RTGSRequest request)
        {
            if (request == null)
            {
                return BadRequest("Datos de RTGS son requeridos");
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

            if (!_auth.HasPermission(sessionId, "OPERAR_RTGS"))
            {
                return Forbidden("No tiene permiso para operar RTGS");
            }

            var kv = new Dictionary<string, string>
            {
                { "origen", request.Origen.ToString() },
                { "destino", request.Destino.ToString() },
                { "monto", request.Monto.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) },
                { "moneda", request.Moneda ?? "DOP" }
            };

            if (!string.IsNullOrWhiteSpace(request.RefExterna))
            {
                kv["ref_externa"] = request.RefExterna;
            }

            if (!string.IsNullOrWhiteSpace(request.Glosa))
            {
                kv["glosa"] = request.Glosa;
            }

            var result = _services.PostRTGS(sessionId, kv);
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

    public class ReversarTransaccionRequest
    {
        public string Motivo { get; set; }
    }

    public class RTGSRequest
    {
        public Guid Origen { get; set; }
        public Guid Destino { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; }
        public string RefExterna { get; set; }
        public string Glosa { get; set; }
    }
}