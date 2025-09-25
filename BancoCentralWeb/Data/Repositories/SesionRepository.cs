using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public class SesionRepository : Repository<Sesion>, ISesionRepository
    {
        public SesionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Sesion?> GetByTokenAsync(string token)
        {
            if (!Guid.TryParse(token, out var sesionId))
                return null;

            return await _dbSet
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == sesionId && s.Activa);
        }

        public async Task<IEnumerable<Sesion>> GetByUsuarioIdAsync(Guid usuarioId)
        {
            return await _dbSet
                .Include(s => s.Usuario)
                .Where(s => s.UsuarioId == usuarioId)
                .OrderByDescending(s => s.EmitidaEn)
                .ToListAsync();
        }

        public async Task<bool> IsValidAsync(Guid sesionId)
        {
            var sesion = await GetByIdAsync(sesionId);
            return sesion?.Activa ?? false;
        }

        public async Task<bool> TerminateSessionAsync(Guid sesionId)
        {
            var sesion = await GetByIdAsync(sesionId);
            if (sesion == null) return false;

            sesion.ExpiraEn = DateTime.UtcNow.AddMinutes(-1);
            Update(sesion);
            await SaveChangesAsync();
            return true;
        }

        public async Task TerminateAllUserSessionsAsync(Guid usuarioId)
        {
            var sesiones = await _dbSet
                .Where(s => s.UsuarioId == usuarioId && s.Activa)
                .ToListAsync();

            foreach (var sesion in sesiones)
            {
                sesion.ExpiraEn = DateTime.UtcNow.AddMinutes(-1);
                Update(sesion);
            }

            await SaveChangesAsync();
        }
    }
}