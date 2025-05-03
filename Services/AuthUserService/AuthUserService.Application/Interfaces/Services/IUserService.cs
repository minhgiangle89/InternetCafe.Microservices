using AuthUserService.Application.DTOs.User;
using InternetCafe.Common.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthUserService.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDTO> AuthenticateUserAsync(string username, string password);
        Task<UserDTO> RegisterUserAsync(CreateUserDTO userDTO);
        Task UpdateUserAsync(int userId, UpdateUserDTO userDTO);
        Task<bool> CheckPasswordAsync(int userId, string password);
        Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO);
        Task ChangeUserStatusAsync(int userId, UserStatus status);
        Task<UserDTO> GetUserByIdAsync(int userId);
        Task<UserDTO> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
    }
}
