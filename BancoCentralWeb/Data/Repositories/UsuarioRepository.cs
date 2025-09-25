using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> GetByUsuarioAsync(string usuario)
        {
            return await _dbSet
                .Include(u => u.Sesiones)
                .FirstOrDefaultAsync(u => u.NombreUsuario == usuario && u.Activo);
        }

        public async Task<Usuario?> GetByCorreoAsync(string correo)
        {
            return await _dbSet
                .Include(u => u.Sesiones)
                .FirstOrDefaultAsync(u => u.Correo == correo && u.Activo);
        }

        public async Task<bool> UsuarioExistsAsync(string usuario)
        {
            return await _dbSet.AnyAsync(u => u.NombreUsuario == usuario);
        }

        public async Task<bool> CorreoExistsAsync(string correo)
        {
            return await _dbSet.AnyAsync(u => u.Correo == correo);
        }

        public async Task<bool> ValidatePasswordAsync(string usuario, string password)
        {
            var user = await GetByUsuarioAsync(usuario);
            if (user == null) return false;

            var passwordHash = HashPassword(password, user.Salt);
            return passwordHash.SequenceEqual(user.PasswordHash);
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}