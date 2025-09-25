using System.Threading.Tasks;

namespace BancoCentralWeb.Data.Repositories
{
    public interface IUnitOfWork
    {
        IClienteRepository Clientes { get; }
        ICuentaRepository Cuentas { get; }
        IUsuarioRepository Usuarios { get; }
        ISesionRepository Sesiones { get; }
        
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}