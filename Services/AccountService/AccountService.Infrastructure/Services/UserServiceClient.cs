using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Services
{
    public interface IUserServiceClient
    {
        Task<UserDto> GetUserByIdAsync(int userId);
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;

        public UserServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserDto>>($"/api/user/{userId}");
            return response?.Data ?? throw new Exception($"User with ID {userId} not found.");
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
    }
}
