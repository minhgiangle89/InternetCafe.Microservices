using AccountService.Application.DTOs.Account;
using AccountService.Application.DTOs.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDTO>> GetAllAccountAsync();
        Task<AccountDTO> CreateAccountAsync(int userId);
        Task<decimal> GetBalanceAsync(int accountId);
        Task<decimal> GetBalanceByUserIdAsync(int userId);
        Task<TransactionDTO> DepositAsync(DepositDTO depositDTO);
        Task<TransactionDTO> WithdrawAsync(WithdrawDTO withdrawDTO);
        Task<TransactionDTO> ChargeForSessionAsync(int accountId, int sessionId, decimal amount);
        Task<bool> HasSufficientBalanceAsync(int accountId, decimal amount);
        Task<AccountDetailsDTO> GetAccountWithTransactionsAsync(int accountId);
        Task<AccountDTO> GetAccountByUserIdAsync(int userId);
        Task<IEnumerable<TransactionDTO>> GetTransactionsByAccountIdAsync(int accountId, int pageNumber, int pageSize);
    }
}
