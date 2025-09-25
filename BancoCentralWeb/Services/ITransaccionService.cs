namespace BancoCentralWeb.Services
{
    public interface ITransaccionService
    {
        Task<Models.Transacciones.TransaccionListResponse?> ListarTransaccionesAsync(int page, int pageSize, Guid sessionId);
        Task<Models.Transacciones.TransaccionResponse?> ObtenerTransaccionAsync(Guid id, Guid sessionId);
    }
}