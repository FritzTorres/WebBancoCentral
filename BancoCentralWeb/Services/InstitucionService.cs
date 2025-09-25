using BancoCentralWeb.Models.Instituciones;

namespace BancoCentralWeb.Services
{
    public class InstitucionService : IInstitucionService
    {
        private readonly IApiService _apiService;

        public InstitucionService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<InstitucionListResponse?> ListarInstitucionesAsync(Guid sessionId)
        {
            return await _apiService.GetAsync<InstitucionListResponse>("instituciones", sessionId);
        }

        public async Task<InstitucionResponse?> ObtenerInstitucionAsync(Guid id, Guid sessionId)
        {
            return await _apiService.GetAsync<InstitucionResponse>($"instituciones/{id}", sessionId);
        }
    }
}