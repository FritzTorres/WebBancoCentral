namespace BancoCentralWeb.Models.Instituciones
{
    public class InstitucionResponse
    {
        public Guid InstitucionId { get; set; }
        public string CodigoSib { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}