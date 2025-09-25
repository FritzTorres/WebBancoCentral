namespace BancoCentralWeb.Models.Clientes
{
    public class ClienteListResponse
    {
        public List<ClienteResponse> Clientes { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}