using AccountService.Domain.Entities;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(int accountId);
        Task<IReadOnlyList<Transaction>> GetBySessionIdAsync(int sessionId);
        Task<decimal> GetTotalAmountByTypeAndDateRangeAsync(TransactionType type, DateTime startDate, DateTime endDate);
    }
}
