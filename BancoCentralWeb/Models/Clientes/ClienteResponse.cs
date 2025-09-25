namespace BancoCentralWeb.Models.Clientes
{
    public class ClienteResponse
    {
        public Guid ClienteId { get; set; }
        public string CedulaRnc { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;
        public DateTime? KycVigenteHasta { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}