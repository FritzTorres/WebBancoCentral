using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cliente?> GetByCedulaRncAsync(string cedulaRnc)
        {
            return await _dbSet
                .Include(c => c.Cuentas)
                .Include(c => c.Certificados)
                .FirstOrDefaultAsync(c => c.CedulaRnc == cedulaRnc);
        }

        public async Task<IEnumerable<Cliente>> GetByTipoClienteAsync(string tipoCliente)
        {
            return await _dbSet
                .Where(c => c.TipoCliente == tipoCliente)
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.ToLower();
            return await _dbSet
                .Where(c => c.NombreCompleto.ToLower().Contains(searchTerm) ||
                           c.CedulaRnc.ToLower().Contains(searchTerm))
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();
        }

        public async Task<bool> CedulaRncExistsAsync(string cedulaRnc)
        {
            return await _dbSet.AnyAsync(c => c.CedulaRnc == cedulaRnc);
        }
    }
}