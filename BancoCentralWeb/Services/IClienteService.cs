namespace BancoCentralWeb.Services
{
    public interface IClienteService
    {
        Task<Models.Clientes.ClienteResponse?> CrearClienteAsync(Models.Clientes.ClienteRequest request, Guid sessionId);
        Task<Models.Clientes.ClienteResponse?> ObtenerClienteAsync(Guid id, Guid sessionId);
        Task<Models.Clientes.ClienteListResponse?> ListarClientesAsync(string? q, int page, int pageSize, Guid sessionId);
        Task<Models.Cuentas.CuentaListResponse?> ObtenerResumenCuentasAsync(Guid clienteId, Guid sessionId);
        Task<bool> EmitirCertificadoSolvenciaAsync(Guid clienteId, Guid sessionId);
    }
}