using Microsoft.AspNetCore.Mvc;
using BancoCentralWeb.Services;
using BancoCentralWeb.Models.Auth;

namespace BancoCentralWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al dashboard
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (!string.IsNullOrEmpty(sessionId) && Guid.TryParse(sessionId, out Guid sessionGuid))
            {
                if (_authService.IsSessionValidAsync(sessionGuid).Result)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            _logger.LogInformation("Login POST iniciado para usuario: {Usuario}", request.Usuario);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido");
                return View(request);
            }

            try
            {
                _logger.LogInformation("Intentando login con AuthService");
                var result = await _authService.LoginAsync(request.Usuario, request.Contrasena);
                
                if (result != null)
                {
                    _logger.LogInformation("Login exitoso, guardando sesión");
                    
                    // Guardar sesión
                    HttpContext.Session.SetString("SessionId", result.SessionId.ToString());
                    HttpContext.Session.SetString("Usuario", request.Usuario);
                    
                    // Verificar que la sesión se guardó correctamente
                    var savedSessionId = HttpContext.Session.GetString("SessionId");
                    var savedUsuario = HttpContext.Session.GetString("Usuario");
                    
                    _logger.LogInformation("Sesión guardada - SessionId: {SessionId}, Usuario: {Usuario}", savedSessionId, savedUsuario);
                    
                    _logger.LogInformation("Redirigiendo a Home/Index");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("Login fallido - result es null");
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                    return View(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                ModelState.AddModelError(string.Empty, $"Error al iniciar sesión: {ex.Message}");
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (!string.IsNullOrEmpty(sessionId) && Guid.TryParse(sessionId, out Guid sessionGuid))
            {
                await _authService.LogoutAsync(sessionGuid);
            }

            // Limpiar sesión
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}