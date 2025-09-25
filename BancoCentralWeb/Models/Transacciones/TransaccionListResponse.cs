namespace BancoCentralWeb.Models.Transacciones
{
    public class TransaccionListResponse
    {
        public List<TransaccionResponse> Transacciones { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}