using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public interface ISesionRepository : IRepository<Sesion>
    {
        Task<Sesion?> GetByTokenAsync(string token);
        Task<IEnumerable<Sesion>> GetByUsuarioIdAsync(Guid usuarioId);
        Task<bool> IsValidAsync(Guid sesionId);
        Task<bool> TerminateSessionAsync(Guid sesionId);
        Task TerminateAllUserSessionsAsync(Guid usuarioId);
    }
}