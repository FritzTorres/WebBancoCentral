using System;
using System.Threading.Tasks;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByUsuarioAsync(string usuario);
        Task<Usuario?> GetByCorreoAsync(string correo);
        Task<bool> UsuarioExistsAsync(string usuario);
        Task<bool> CorreoExistsAsync(string correo);
        Task<bool> ValidatePasswordAsync(string usuario, string password);
    }
}