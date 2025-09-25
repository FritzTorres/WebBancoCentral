using Microsoft.AspNetCore.Mvc;
using BancoCentralWeb.Services;
using BancoCentralWeb.Models.Clientes;
using BancoCentralWeb.Filters;

namespace BancoCentralWeb.Controllers
{
    [TypeFilter(typeof(SessionAuthorizeAttribute))]
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        private Guid? GetSessionId()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(sessionId) || !Guid.TryParse(sessionId, out Guid sessionGuid))
            {
                return null;
            }
            return sessionGuid;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            var sessionId = GetSessionId();
            if (sessionId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var clientes = await _clienteService.ListarClientesAsync(q, page, pageSize, sessionId.Value);
                return View(clientes);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar clientes: {ex.Message}");
                return View(new Models.Clientes.ClienteListResponse());
            }
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(ClienteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var sessionId = GetSessionId();
            if (sessionId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = await _clienteService.CrearClienteAsync(request, sessionId.Value);
                if (result != null)
                {
                    TempData["Success"] = "Cliente creado exitosamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al crear el cliente");
                    return View(request);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al crear el cliente: {ex.Message}");
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalles(Guid id)
        {
            var sessionId = GetSessionId();
            if (sessionId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var cliente = await _clienteService.ObtenerClienteAsync(id, sessionId.Value);
                if (cliente != null)
                {
                    var cuentas = await _clienteService.ObtenerResumenCuentasAsync(id, sessionId.Value);
                    ViewBag.Cuentas = cuentas;
                    return View(cliente);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar el cliente: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmitirCertificadoSolvencia(Guid id)
        {
            var sessionId = GetSessionId();
            if (sessionId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = await _clienteService.EmitirCertificadoSolvenciaAsync(id, sessionId.Value);
                if (result)
                {
                    TempData["Success"] = "Certificado de solvencia emitido exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al emitir el certificado de solvencia";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al emitir el certificado: {ex.Message}";
            }

            return RedirectToAction("Detalles", new { id });
        }
    }
}