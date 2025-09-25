using BancoCentralWeb.Models.Cuentas;

namespace BancoCentralWeb.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly IApiService _apiService;

        public CuentaService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<CuentaResponse?> AbrirCuentaAsync(CuentaRequest request, Guid sessionId)
        {
            return await _apiService.PostAsync<CuentaResponse>("cuentas", request, sessionId);
        }

        public async Task<CuentaResponse?> ObtenerCuentaAsync(Guid id, Guid sessionId)
        {
            return await _apiService.GetAsync<CuentaResponse>($"cuentas/{id}", sessionId);
        }

        public async Task<CuentaListResponse?> ListarCuentasAsync(int page, int pageSize, Guid sessionId)
        {
            return await _apiService.GetAsync<CuentaListResponse>($"cuentas?page={page}&page_size={pageSize}", sessionId);
        }
    }
}