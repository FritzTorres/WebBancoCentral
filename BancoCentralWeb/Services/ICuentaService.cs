namespace BancoCentralWeb.Services
{
    public interface ICuentaService
    {
        Task<Models.Cuentas.CuentaResponse?> AbrirCuentaAsync(Models.Cuentas.CuentaRequest request, Guid sessionId);
        Task<Models.Cuentas.CuentaResponse?> ObtenerCuentaAsync(Guid id, Guid sessionId);
        Task<Models.Cuentas.CuentaListResponse?> ListarCuentasAsync(int page, int pageSize, Guid sessionId);
    }
}