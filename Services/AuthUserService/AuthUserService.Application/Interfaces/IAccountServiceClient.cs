using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthUserService.Application.Interfaces
{
    public interface IAccountServiceClient
    {
        Task CreateAccountAsync(int userId);
    }
}
