using ComputerSessionService.Domain.Entities;
using InternetCafe.Common.Repositories.IRepositories;

namespace ComputerSessionService.Application.Interfaces.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        Task<Session?> GetCurrentSessionByComputerIdAsync(int computerId);
        Task<IReadOnlyList<Session>> GetActiveSessionsAsync();
        Task<IReadOnlyList<Session>> GetByUserIdAsync(int userId);
        Task<IReadOnlyList<Session>> GetByComputerIdAsync(int computerId);
    }
}
