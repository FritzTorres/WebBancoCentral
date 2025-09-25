namespace BancoCentralWeb.Models.Cuentas
{
    public class CuentaRequest
    {
        public Guid? ClienteId { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
    }
}