using Microsoft.AspNetCore.Mvc;
using BancoCentralWeb.Services;
using BancoCentralWeb.Models.Certificados;

namespace BancoCentralWeb.Controllers
{
    public class CertificadosController : Controller
    {
        private readonly ICertificadoService _certificadoService;
        private readonly IAuthService _authService;

        public CertificadosController(ICertificadoService certificadoService, IAuthService authService)
        {
            _certificadoService = certificadoService;
            _authService = authService;
        }

        private async Task<bool> ValidarSesionAsync()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(sessionId) || !Guid.TryParse(sessionId, out Guid sessionGuid))
            {
                return false;
            }

            return await _authService.IsSessionValidAsync(sessionGuid);
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
        public async Task<IActionResult> Index()
        {
            if (!await ValidarSesionAsync())
            {
                return RedirectToAction("Login", "Auth");
            }

            var sessionId = GetSessionId();
            if (sessionId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var certificados = await _certificadoService.ListarCertificadosAsync(sessionId.Value);
                return View(certificados);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar certificados: {ex.Message}");
                return View(new Models.Certificados.CertificadoListResponse());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalles(Guid id)
        {
            if (!await ValidarSesionAsync())
            {
                return RedirectToAction("Login", "Auth");
            }

            var sessionId = GetSessionId();
            if (sessionId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var certificado = await _certificadoService.ObtenerCertificadoAsync(id, sessionId.Value);
                if (certificado != null)
                {
                    return View(certificado);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar el certificado: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmitirCertificadoSaldo(Guid cuentaId)
        {
            if (!await ValidarSesionAsync())
            {
                return RedirectToAction("Login", "Auth");
            }

            var sessionId = GetSessionId();
            if (sessionId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = await _certificadoService.EmitirCertificadoSaldoAsync(cuentaId, sessionId.Value);
                if (result)
                {
                    TempData["Success"] = "Certificado de saldo emitido exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al emitir el certificado de saldo";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al emitir el certificado: {ex.Message}";
            }

            return RedirectToAction("Detalles", "Cuentas", new { id = cuentaId });
        }
    }
}