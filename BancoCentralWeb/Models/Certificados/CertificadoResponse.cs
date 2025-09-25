namespace BancoCentralWeb.Models.Certificados
{
    public class CertificadoResponse
    {
        public Guid CertificadoId { get; set; }
        public Guid? ClienteId { get; set; }
        public Guid? CuentaId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public Guid EmitidoPorUsuarioId { get; set; }
        public DateTime EmitidoEn { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Hash { get; set; }
    }
}