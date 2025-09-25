using BancoCentralWeb.Models.Transacciones;

namespace BancoCentralWeb.Services
{
    public class TransaccionService : ITransaccionService
    {
        private readonly IApiService _apiService;

        public TransaccionService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<TransaccionListResponse?> ListarTransaccionesAsync(int page, int pageSize, Guid sessionId)
        {
            return await _apiService.GetAsync<TransaccionListResponse>($"transacciones?page={page}&page_size={pageSize}", sessionId);
        }

        public async Task<TransaccionResponse?> ObtenerTransaccionAsync(Guid id, Guid sessionId)
        {
            return await _apiService.GetAsync<TransaccionResponse>($"transacciones/{id}", sessionId);
        }
    }
}