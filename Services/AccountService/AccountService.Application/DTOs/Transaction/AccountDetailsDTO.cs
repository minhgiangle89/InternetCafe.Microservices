using AccountService.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.Transaction
{
    public class AccountDetailsDTO : AccountDTO
    {
        public ICollection<TransactionDTO> RecentTransactions { get; set; } = new List<TransactionDTO>();
    }
}
