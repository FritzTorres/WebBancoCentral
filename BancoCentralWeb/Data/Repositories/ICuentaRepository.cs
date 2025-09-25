using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public interface ICuentaRepository : IRepository<Cuenta>
    {
        Task<IEnumerable<Cuenta>> GetByClienteIdAsync(Guid clienteId);
        Task<IEnumerable<Cuenta>> GetByInstitucionIdAsync(Guid institucionId);
        Task<Cuenta?> GetByProductoAsync(Guid clienteId, string producto);
        Task<decimal> GetSaldoAsync(Guid cuentaId);
        Task<IEnumerable<Cuenta>> GetActivasByClienteAsync(Guid clienteId);
    }
}