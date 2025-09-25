using Microsoft.AspNetCore.Mvc;
using BancoCentralWeb.Services;
using BancoCentralWeb.Models.Transacciones;

namespace BancoCentralWeb.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly ITransaccionService _transaccionService;
        private readonly IAuthService _authService;

        public TransaccionesController(ITransaccionService transaccionService, IAuthService authService)
        {
            _transaccionService = transaccionService;
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
                var transacciones = await _transaccionService.ListarTransaccionesAsync(page, pageSize, sessionId.Value);
                return View(transacciones);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar transacciones: {ex.Message}");
                return View(new Models.Transacciones.TransaccionListResponse());
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
                var transaccion = await _transaccionService.ObtenerTransaccionAsync(id, sessionId.Value);
                if (transaccion != null)
                {
                    return View(transaccion);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar la transacción: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}