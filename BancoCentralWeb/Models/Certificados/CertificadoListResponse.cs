namespace BancoCentralWeb.Models.Certificados
{
    public class CertificadoListResponse
    {
        public List<CertificadoResponse> Certificados { get; set; } = new();
        public int Total { get; set; }
    }
}