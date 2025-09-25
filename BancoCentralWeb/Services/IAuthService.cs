namespace BancoCentralWeb.Services
{
    public interface IAuthService
    {
        Task<Models.Auth.LoginResponse?> LoginAsync(string usuario, string contrasena);
        Task<bool> LogoutAsync(Guid sessionId);
        Task<bool> IsSessionValidAsync(Guid sessionId);
        Task<bool> HasPermissionAsync(Guid sessionId, string permisoClave);
        Task<Guid> GetUserIdAsync(Guid sessionId);
    }
}