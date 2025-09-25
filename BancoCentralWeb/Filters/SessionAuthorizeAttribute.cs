using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BancoCentralWeb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BancoCentralWeb.Filters
{
    public class SessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly ILogger<SessionAuthorizeAttribute> _logger;

        public SessionAuthorizeAttribute(ILogger<SessionAuthorizeAttribute> logger)
        {
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
            if (authService == null)
            {
                _logger.LogError("No se pudo obtener el servicio IAuthService");
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var sessionId = context.HttpContext.Session.GetString("SessionId");
            
            _logger.LogInformation("SessionAuthorizeAttribute - SessionId recuperado: {SessionId}", sessionId);
            
            if (string.IsNullOrEmpty(sessionId) || !Guid.TryParse(sessionId, out Guid sessionGuid))
            {
                _logger.LogWarning("SessionAuthorizeAttribute - SessionId inválido o nulo, redirigiendo a Login");
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            _logger.LogInformation("SessionAuthorizeAttribute - Validando sesión: {SessionGuid}", sessionGuid);
            var isValid = await authService.IsSessionValidAsync(sessionGuid);
            _logger.LogInformation("SessionAuthorizeAttribute - Sesión válida: {IsValid}", isValid);
            
            if (!isValid)
            {
                // Limpiar sesión inválida
                _logger.LogWarning("SessionAuthorizeAttribute - Sesión inválida, limpiando y redirigiendo a Login");
                context.HttpContext.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }
            
            _logger.LogInformation("SessionAuthorizeAttribute - Sesión válida, permitiendo acceso");
        }
    }
}