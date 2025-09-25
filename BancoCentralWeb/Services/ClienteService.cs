using BancoCentralWeb.Models.Clientes;
using BancoCentralWeb.Models.Cuentas;

namespace BancoCentralWeb.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IApiService _apiService;

        public ClienteService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ClienteResponse?> CrearClienteAsync(ClienteRequest request, Guid sessionId)
        {
            return await _apiService.PostAsync<ClienteResponse>("clientes", request, sessionId);
        }

        public async Task<ClienteResponse?> ObtenerClienteAsync(Guid id, Guid sessionId)
        {
            return await _apiService.GetAsync<ClienteResponse>($"clientes/{id}", sessionId);
        }

        public async Task<ClienteListResponse?> ListarClientesAsync(string? q, int page, int pageSize, Guid sessionId)
        {
            var queryString = $"?page={page}&page_size={pageSize}";
            if (!string.IsNullOrWhiteSpace(q))
            {
                queryString += $"&q={Uri.EscapeDataString(q)}";
            }
            
            return await _apiService.GetAsync<ClienteListResponse>($"clientes{queryString}", sessionId);
        }

        public async Task<CuentaListResponse?> ObtenerResumenCuentasAsync(Guid clienteId, Guid sessionId)
        {
            return await _apiService.GetAsync<CuentaListResponse>($"clientes/{clienteId}/cuentas", sessionId);
        }

        public async Task<bool> EmitirCertificadoSolvenciaAsync(Guid clienteId, Guid sessionId)
        {
            var result = await _apiService.PostAsync<Models.Base.BaseResponse>($"clientes/{clienteId}/certificados/solvencia", new { }, sessionId);
            return result?.Success ?? false;
        }
    }
}