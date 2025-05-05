using ComputerSessionService.Application.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Infrastructure.Services
{
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;

        public AccountServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<bool> ChargeForSessionAsync(int accountId, int sessionId, decimal amount)
        {
            var request = new
            {
                AccountId = accountId,
                SessionId = sessionId,
                Amount = amount
            };

            var response = await _httpClient.PostAsJsonAsync("/api/account/charge", request);
            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<decimal> GetAccountBalanceAsync(int userId)
        {
            var response = await _httpClient.GetFromJsonAsync<AccountBalanceResponse>($"/api/account/user/{userId}/balance");
            return response?.Balance ?? 0;
        }

        private class AccountBalanceResponse
        {
            public decimal Balance { get; set; }
        }
    }
}
