using ComputerSessionService.Application.Interfaces.Repositories;
using ComputerSessionService.Domain.Entities;
using ComputerSessionService.Infrastructure.Persistence;
using InternetCafe.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComputerSessionService.Infrastructure.Repositories
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        public SessionRepository(ComputerSessionDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Session?> GetCurrentSessionByComputerIdAsync(int computerId)
        {
            return await _dbSet
                .Where(s => s.ComputerId == computerId && s.Status == SessionStatus.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Session>> GetActiveSessionsAsync()
        {
            return await _dbSet
                .Where(s => s.Status == SessionStatus.Active)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetByComputerIdAsync(int computerId)
        {
            return await _dbSet
                .Where(s => s.ComputerId == computerId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }
    }
}
