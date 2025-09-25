using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public class CuentaRepository : Repository<Cuenta>, ICuentaRepository
    {
        public CuentaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Cuenta>> GetByClienteIdAsync(Guid clienteId)
        {
            return await _dbSet
                .Include(c => c.Cliente)
                .Include(c => c.Institucion)
                .Include(c => c.Transacciones)
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.AbiertaEn)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cuenta>> GetByInstitucionIdAsync(Guid institucionId)
        {
            return await _dbSet
                .Include(c => c.Cliente)
                .Include(c => c.Institucion)
                .Where(c => c.InstitucionId == institucionId)
                .OrderBy(c => c.Producto)
                .ToListAsync();
        }

        public async Task<Cuenta?> GetByProductoAsync(Guid clienteId, string producto)
        {
            return await _dbSet
                .Include(c => c.Cliente)
                .Include(c => c.Institucion)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Producto == producto);
        }

        public async Task<decimal> GetSaldoAsync(Guid cuentaId)
        {
            var cuenta = await GetByIdAsync(cuentaId);
            return cuenta?.Saldo ?? 0;
        }

        public async Task<IEnumerable<Cuenta>> GetActivasByClienteAsync(Guid clienteId)
        {
            return await _dbSet
                .Include(c => c.Cliente)
                .Include(c => c.Institucion)
                .Where(c => c.ClienteId == clienteId && c.Estado == "activa")
                .OrderByDescending(c => c.AbiertaEn)
                .ToListAsync();
        }
    }
}