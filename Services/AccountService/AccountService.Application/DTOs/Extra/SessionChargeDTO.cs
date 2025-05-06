using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.Extra
{
    public class SessionChargeDTO
    {
        public int AccountId { get; set; }
        public int SessionId { get; set; }
        public decimal Amount { get; set; }
    }
}
