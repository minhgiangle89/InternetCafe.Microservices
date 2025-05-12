using AuthUserService.Application.DTOs.User;
using AuthUserService.Application.Interfaces;
using AuthUserService.Application.Interfaces.Services;
using AuthUserService.Domain.Entities;
using AuthUserService.Domain.Exceptions;
using AutoMapper;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;


namespace AuthUserService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<UserService> _logger;
        private readonly IAccountServiceClient _accountServiceClient;

        public UserService(IUnitOfWork unitOfWork,
                            IPasswordHasher passwordHasher,
                            IMapper mapper,
                            IAuditLogger auditLogger,
                            ILogger<UserService> logger,
                            IAccountServiceClient accountServiceClient)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountServiceClient = accountServiceClient ?? throw new ArgumentNullException(nameof(accountServiceClient));
        }

        public async Task<UserDTO> AuthenticateUserAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
            {
                throw new UserNotFoundException(username);
            }

            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                throw new AuthenticationException("Invalid password");
            }

            user.LastLoginTime = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> RegisterUserAsync(CreateUserDTO userDTO)
        {
            try
            {
                var existingUserWithUsername = await _unitOfWork.Users.GetByUsernameAsync(userDTO.Username);
                if (existingUserWithUsername != null)
                {
                    throw new DuplicateUserException("username", userDTO.Username);
                }

                var existingUserWithEmail = await _unitOfWork.Users.GetByEmailAsync(userDTO.Email);
                if (existingUserWithEmail != null)
                {
                    throw new DuplicateUserException("email", userDTO.Email);
                }

                var user = new User
                {
                    Username = userDTO.Username,
                    Email = userDTO.Email,
                    PasswordHash = _passwordHasher.HashPassword(userDTO.Password),
                    FullName = userDTO.FullName,
                    PhoneNumber = userDTO.PhoneNumber,
                    Address = userDTO.Address,
                    DateOfBirth = userDTO.DateOfBirth,
                    Role = userDTO.Role,
                    LastLoginTime = DateTime.UtcNow,
                    Status = (int)UserStatus.Active
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                try
                {
                    await _accountServiceClient.CreateAccountAsync(user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể tạo tài khoản cho user {UserId}", user.Id);
                }

                await _auditLogger.LogActivityAsync(
                    "UserCreated",
                    "User",
                    user.Id,
                    0, // System operation
                    DateTime.UtcNow,
                    $"User {user.Username} was created");

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex) when (ex is not DuplicateUserException)
            {
                _logger.LogError(ex, "Error registering user: {Username}", userDTO.Username);
                throw;
            }
        }

        public async Task UpdateUserAsync(int userId, UpdateUserDTO userDTO)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Email = userDTO.Email;
            user.FullName = userDTO.FullName;
            user.PhoneNumber = userDTO.PhoneNumber;
            user.Address = userDTO.Address;
            user.DateOfBirth = userDTO.DateOfBirth;
            user.Role  = userDTO.Role;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogActivityAsync(
                "UserUpdated",
                "User",
                userId,
                userId,
                DateTime.UtcNow,
                $"User {user.Username} was updated");
        }

        public async Task<bool> CheckPasswordAsync(int userId, string password)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            return _passwordHasher.VerifyPassword(password, user.PasswordHash);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            if (!_passwordHasher.VerifyPassword(changePasswordDTO.CurrentPassword, user.PasswordHash))
            {
                throw new AuthenticationException("Current password is incorrect");
            }

            user.PasswordHash = _passwordHasher.HashPassword(changePasswordDTO.NewPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogActivityAsync(
                "PasswordChanged",
                "User",
                userId,
                userId,
                DateTime.UtcNow,
                "Password was changed");
        }

        public async Task ChangeUserStatusAsync(int userId, UserStatus status)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Status = (int)status;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogActivityAsync(
                "UserStatusChanged",
                "User",
                userId,
                userId,
                DateTime.UtcNow,
                $"User status changed to {status}");
        }

        public async Task<UserDTO> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> GetUserByUsernameAsync(string username)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
            {
                throw new UserNotFoundException(username);
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }
    }
}
