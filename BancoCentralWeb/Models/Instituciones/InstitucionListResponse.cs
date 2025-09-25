namespace BancoCentralWeb.Models.Instituciones
{
    public class InstitucionListResponse
    {
        public List<InstitucionResponse> Instituciones { get; set; } = new();
        public int Total { get; set; }
    }
}