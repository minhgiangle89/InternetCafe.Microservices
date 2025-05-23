﻿using ComputerSessionService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Infrastructure.Services
{
    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountServiceClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(IHttpContextAccessor));
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }
        }

        public async Task<bool> ChargeForSessionAsync(int accountId, int sessionId, decimal amount)
        {
            AddAuthorizationHeader();
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
            AddAuthorizationHeader();
            var response = await _httpClient.GetFromJsonAsync<AccountBalanceResponse>($"/api/account/user/{userId}/balance");
            return response?.Balance ?? 0;
        }

        private class AccountBalanceResponse
        {
            public decimal Balance { get; set; }
        }
    }
}
