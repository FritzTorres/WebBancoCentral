using Microsoft.AspNetCore.Mvc;
using BancoCentralWeb.Services;
using BancoCentralWeb.Models.Cuentas;

namespace BancoCentralWeb.Controllers
{
    public class CuentasController : Controller
    {
        private readonly ICuentaService _cuentaService;
        private readonly IAuthService _authService;

        public CuentasController(ICuentaService cuentaService, IAuthService authService)
        {
            _cuentaService = cuentaService;
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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
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
                var cuentas = await _cuentaService.ListarCuentasAsync(page, pageSize, sessionId.Value);
                return View(cuentas);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar cuentas: {ex.Message}");
                return View(new Models.Cuentas.CuentaListResponse());
            }
        }

        [HttpGet]
        public IActionResult Abrir()
        {
            if (!ValidarSesionAsync().Result)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Abrir(CuentaRequest request)
        {
            if (!await ValidarSesionAsync())
            {
                return RedirectToAction("Login", "Auth");
            }

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
                var result = await _cuentaService.AbrirCuentaAsync(request, sessionId.Value);
                if (result != null)
                {
                    TempData["Success"] = "Cuenta abierta exitosamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al abrir la cuenta");
                    return View(request);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al abrir la cuenta: {ex.Message}");
                return View(request);
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
                var cuenta = await _cuentaService.ObtenerCuentaAsync(id, sessionId.Value);
                if (cuenta != null)
                {
                    return View(cuenta);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar la cuenta: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}