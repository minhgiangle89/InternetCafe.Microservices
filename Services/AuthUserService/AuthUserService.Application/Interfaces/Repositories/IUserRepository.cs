using AuthUserService.Domain.Entities;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Repositories.IRepositories;

namespace AuthUserService.Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IReadOnlyList<User>> GetUsersByRoleAsync(UserRole role);
    }
}
