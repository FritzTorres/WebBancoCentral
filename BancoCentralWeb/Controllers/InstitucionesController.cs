using Microsoft.AspNetCore.Mvc;
using BancoCentralWeb.Services;
using BancoCentralWeb.Models.Instituciones;

namespace BancoCentralWeb.Controllers
{
    public class InstitucionesController : Controller
    {
        private readonly IInstitucionService _institucionService;
        private readonly IAuthService _authService;

        public InstitucionesController(IInstitucionService institucionService, IAuthService authService)
        {
            _institucionService = institucionService;
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
                var instituciones = await _institucionService.ListarInstitucionesAsync(sessionId.Value);
                return View(instituciones);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar instituciones: {ex.Message}");
                return View(new Models.Instituciones.InstitucionListResponse());
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
                var institucion = await _institucionService.ObtenerInstitucionAsync(id, sessionId.Value);
                if (institucion != null)
                {
                    return View(institucion);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar la institución: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}