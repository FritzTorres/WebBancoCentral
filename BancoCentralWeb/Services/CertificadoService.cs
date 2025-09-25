using BancoCentralWeb.Models.Certificados;

namespace BancoCentralWeb.Services
{
    public class CertificadoService : ICertificadoService
    {
        private readonly IApiService _apiService;

        public CertificadoService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<CertificadoListResponse?> ListarCertificadosAsync(Guid sessionId)
        {
            return await _apiService.GetAsync<CertificadoListResponse>("certificados", sessionId);
        }

        public async Task<CertificadoResponse?> ObtenerCertificadoAsync(Guid id, Guid sessionId)
        {
            return await _apiService.GetAsync<CertificadoResponse>($"certificados/{id}", sessionId);
        }

        public async Task<bool> EmitirCertificadoSaldoAsync(Guid cuentaId, Guid sessionId)
        {
            var result = await _apiService.PostAsync<Models.Base.BaseResponse>($"certificados/saldo", new { cuentaId }, sessionId);
            return result?.Success ?? false;
        }
    }
}