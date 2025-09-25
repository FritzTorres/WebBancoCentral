namespace BancoCentralWeb.Models.Transacciones
{
    public class TransaccionResponse
    {
        public Guid TransaccionId { get; set; }
        public string? RefExterna { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime CreadaEn { get; set; }
        public DateTime? ValidadaEn { get; set; }
        public DateTime? ContabilizadaEn { get; set; }
        public decimal MontoTotal { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public string? Glosa { get; set; }
    }
}