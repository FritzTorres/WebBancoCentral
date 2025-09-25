namespace BancoCentralWeb.Services
{
    public interface IInstitucionService
    {
        Task<Models.Instituciones.InstitucionListResponse?> ListarInstitucionesAsync(Guid sessionId);
        Task<Models.Instituciones.InstitucionResponse?> ObtenerInstitucionAsync(Guid id, Guid sessionId);
    }
}