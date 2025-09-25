namespace BancoCentralWeb.Models.Cuentas
{
    public class CuentaResponse
    {
        public Guid CuentaId { get; set; }
        public Guid? ClienteId { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime AbiertaEn { get; set; }
        public decimal Saldo { get; set; }
    }
}