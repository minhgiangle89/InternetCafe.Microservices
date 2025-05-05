using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.Interfaces
{
    public interface IAccountServiceClient
    {
        Task<bool> ChargeForSessionAsync(int accountId, int sessionId, decimal amount);
        Task<decimal> GetAccountBalanceAsync(int userId);
    }
}
