namespace BancoCentralWeb.Models.Cuentas
{
    public class CuentaListResponse
    {
        public List<CuentaResponse> Cuentas { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}