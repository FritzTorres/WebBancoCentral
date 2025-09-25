using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using BancoCentralRD.Web.Models.Core;
using BancoCentralRD.Web.Models.DTOs;

namespace BancoCentralRD.Web.Controllers
{
    public class InstitucionesController : ApiController
    {
        private readonly Services _services;
        private readonly Envios _envios;
        private readonly Auth _auth;

        public InstitucionesController()
        {
            _services = new Services();
            _envios = new Envios();
            _auth = new Auth();
        }

        [HttpPost]
        [Route("api/instituciones")]
        public IHttpActionResult CrearInstitucion([FromBody] InstitucionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Datos de la institución son requeridos");
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

            if (!_auth.HasPermission(sessionId, "CONFIG_PARAMETROS"))
            {
                return Forbidden("No tiene permiso para crear instituciones");
            }

            var kv = new Dictionary<string, string>
            {
                { "codigo_sib", request.CodigoSib },
                { "nombre", request.Nombre },
                { "tipo", request.Tipo }
            };

            var result = _services.CrearInstitucion(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/instituciones/{id}")]
        public IHttpActionResult ObtenerInstitucion(Guid id)
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

            if (!_auth.HasPermission(sessionId, "CONSULTAR_INSTITUCIONES"))
            {
                return Forbidden("No tiene permiso para consultar instituciones");
            }

            var kv = new Dictionary<string, string>
            {
                { "institucion_id", id.ToString() }
            };

            var result = _envios.GetInstitucion(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/instituciones")]
        public IHttpActionResult ObtenerInstitucionPorCodigo([FromUri] string codigoSib)
        {
            if (string.IsNullOrWhiteSpace(codigoSib))
            {
                return BadRequest("Código SIB es requerido");
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

            if (!_auth.HasPermission(sessionId, "CONSULTAR_INSTITUCIONES"))
            {
                return Forbidden("No tiene permiso para consultar instituciones");
            }

            var kv = new Dictionary<string, string>
            {
                { "codigo_sib", codigoSib }
            };

            var result = _envios.GetInstitucion(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/instituciones/{id}/encaje")]
        public IHttpActionResult ObtenerEncaje(Guid id, [FromUri] DateTime fecha)
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

            if (!_auth.HasPermission(sessionId, "CONSULTAR_ENCAJE"))
            {
                return Forbidden("No tiene permiso para consultar encaje");
            }

            var kv = new Dictionary<string, string>
            {
                { "institucion_id", id.ToString() },
                { "fecha", fecha.ToString("yyyy-MM-dd") }
            };

            var result = _envios.GetEncaje(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpPost]
        [Route("api/tiposcambio")]
        public IHttpActionResult RegistrarTipoCambio([FromBody] TipoCambioRequest request)
        {
            if (request == null)
            {
                return BadRequest("Datos del tipo de cambio son requeridos");
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

            if (!_auth.HasPermission(sessionId, "CONFIG_PARAMETROS"))
            {
                return Forbidden("No tiene permiso para registrar tipos de cambio");
            }

            var kv = new Dictionary<string, string>
            {
                { "moneda", request.Moneda },
                { "fecha", request.Fecha.ToString("yyyy-MM-dd") },
                { "tc_compra", request.TcCompra.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) },
                { "tc_venta", request.TcVenta.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) }
            };

            var result = _services.RegistrarTC(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/parametros")]
        public IHttpActionResult ObtenerParametro([FromUri] string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
            {
                return BadRequest("Clave del parámetro es requerida");
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

            if (!_auth.HasPermission(sessionId, "CONFIG_PARAMETROS"))
            {
                return Forbidden("No tiene permiso para consultar parámetros");
            }

            var kv = new Dictionary<string, string>
            {
                { "clave", clave }
            };

            var result = _envios.GetParametro(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpPost]
        [Route("api/parametros")]
        public IHttpActionResult EstablecerParametro([FromBody] ParametroRequest request)
        {
            if (request == null)
            {
                return BadRequest("Datos del parámetro son requeridos");
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

            if (!_auth.HasPermission(sessionId, "CONFIG_PARAMETROS"))
            {
                return Forbidden("No tiene permiso para establecer parámetros");
            }

            var kv = new Dictionary<string, string>
            {
                { "clave", request.Clave },
                { "valor", request.Valor }
            };

            var result = _services.SetParametro(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/reservas")]
        public IHttpActionResult ObtenerReservas()
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

            if (!_auth.HasPermission(sessionId, "CONSULTAR_RESERVAS"))
            {
                return Forbidden("No tiene permiso para consultar reservas");
            }

            var kv = new Dictionary<string, string>();
            var result = _envios.GetReservas(sessionId, kv);
            return ProcessResult(result);
        }

        [HttpGet]
        [Route("api/indicadores")]
        public IHttpActionResult ObtenerIndicadores([FromUri] DateTime desde, [FromUri] DateTime hasta)
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

            if (!_auth.HasPermission(sessionId, "CONSULTAR_INDICADORES"))
            {
                return Forbidden("No tiene permiso para consultar indicadores");
            }

            var kv = new Dictionary<string, string>
            {
                { "desde", desde.ToString("yyyy-MM-dd") },
                { "hasta", hasta.ToString("yyyy-MM-dd") }
            };

            var result = _envios.GetIndicadores(sessionId, kv);
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

    public class TipoCambioRequest
    {
        public string Moneda { get; set; }
        public DateTime Fecha { get; set; }
        public decimal TcCompra { get; set; }
        public decimal TcVenta { get; set; }
    }

    public class ParametroRequest
    {
        public string Clave { get; set; }
        public string Valor { get; set; }
    }
}