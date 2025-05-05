using ComputerSessionService.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IComputerRepository Computers { get; }
        ISessionRepository Sessions { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
