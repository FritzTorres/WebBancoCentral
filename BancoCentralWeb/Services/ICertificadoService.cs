namespace BancoCentralWeb.Services
{
    public interface ICertificadoService
    {
        Task<Models.Certificados.CertificadoListResponse?> ListarCertificadosAsync(Guid sessionId);
        Task<Models.Certificados.CertificadoResponse?> ObtenerCertificadoAsync(Guid id, Guid sessionId);
        Task<bool> EmitirCertificadoSaldoAsync(Guid cuentaId, Guid sessionId);
    }
}