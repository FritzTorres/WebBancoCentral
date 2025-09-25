namespace BancoCentralWeb.Models.Clientes
{
    public class ClienteRequest
    {
        public string CedulaRnc { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;
        public DateTime? KycVigenteHasta { get; set; }
    }
}