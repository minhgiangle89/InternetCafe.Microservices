using ComputerSessionService.Application.Interfaces.Repositories;
using ComputerSessionService.Domain.Entities;
using ComputerSessionService.Infrastructure.Persistence;
using InternetCafe.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Infrastructure.Repositories
{
    public class ComputerRepository : GenericRepository<Computer>, IComputerRepository
    {
        public ComputerRepository(ComputerSessionDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Computer>> GetAvailableComputersAsync()
        {
            return await _dbSet.Where(c => c.ComputerStatus == (int)ComputerStatus.Available)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Computer>> GetByStatusAsync(ComputerStatus status)
        {
            return await _dbSet.Where(c => c.ComputerStatus == (int)status)
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(int computerId, ComputerStatus status)
        {
            var computer = await _dbSet.FindAsync(computerId);
            if (computer != null)
            {
                computer.ComputerStatus = (int)status;
                _dbContext.Entry(computer).Property(c => c.ComputerStatus).IsModified = true;
            }
        }
    }
}
