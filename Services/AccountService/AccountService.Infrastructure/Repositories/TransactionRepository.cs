using AccountService.Application.Interfaces.Repositories;
using AccountService.Domain.Entities;
using AccountService.Infrastructure.Persistence;
using InternetCafe.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AccountDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Creation_Timestamp)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Transaction>> GetBySessionIdAsync(int sessionId)
        {
            return await _dbSet
                .Where(t => t.SessionId == sessionId)
                .OrderByDescending(t => t.Creation_Timestamp)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByTypeAndDateRangeAsync(TransactionType type, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(t => t.Type == type &&
                            t.Creation_Timestamp >= startDate &&
                            t.Creation_Timestamp <= endDate)
                .SumAsync(t => t.Amount);
        }
    }
}
