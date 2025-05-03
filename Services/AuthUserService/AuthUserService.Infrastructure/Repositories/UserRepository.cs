
using AuthUserService.Application.Interfaces.Repositories;
using AuthUserService.Domain.Entities;
using AuthUserService.Infrastructure.Persistence;
using InternetCafe.Common.Enums;
using Microsoft.EntityFrameworkCore;


namespace AuthUserService.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AuthUserDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _dbSet
                .Where(u => u.Role == (int)role)
                .ToListAsync();
        }
    }
}
