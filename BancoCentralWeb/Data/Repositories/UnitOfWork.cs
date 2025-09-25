using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using BancoCentralWeb.Data.Entities;

namespace BancoCentralWeb.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Clientes = new ClienteRepository(context);
            Cuentas = new CuentaRepository(context);
            Usuarios = new UsuarioRepository(context);
            Sesiones = new SesionRepository(context);
        }

        public IClienteRepository Clientes { get; }
        public ICuentaRepository Cuentas { get; }
        public IUsuarioRepository Usuarios { get; }
        public ISesionRepository Sesiones { get; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}