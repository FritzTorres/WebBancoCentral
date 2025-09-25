using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByCedulaRncAsync(string cedulaRnc);
        Task<IEnumerable<Cliente>> GetByTipoClienteAsync(string tipoCliente);
        Task<IEnumerable<Cliente>> SearchAsync(string searchTerm);
        Task<bool> CedulaRncExistsAsync(string cedulaRnc);
    }
}