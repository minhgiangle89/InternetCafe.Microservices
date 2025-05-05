using ComputerSessionService.Domain.Entities;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Repositories.IRepositories;


namespace ComputerSessionService.Application.Interfaces.Repositories
{
    public interface IComputerRepository : IRepository<Computer>
    {
        Task<IReadOnlyList<Computer>> GetAvailableComputersAsync();
        Task<IReadOnlyList<Computer>> GetByStatusAsync(ComputerStatus status);
        Task UpdateStatusAsync(int computerId, ComputerStatus status);
    }
}
