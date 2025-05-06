using AccountService.Domain.Entities;
using InternetCafe.Common.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<Account?> GetByUserIdAsync(int userId);
        Task<Account?> GetWithTransactionsAsync(int accountId);
    }
}
