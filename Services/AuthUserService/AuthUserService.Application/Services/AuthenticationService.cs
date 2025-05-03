﻿using AuthUserService.Application.DTOs.Authentication;
using AuthUserService.Application.Interfaces.Repositories;
using AuthUserService.Application.Interfaces.Services;
using AuthUserService.Application.Interfaces;
using AuthUserService.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace AuthUserService.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserRepository userRepository,
            ITokenGenerator tokenGenerator,
            IPasswordHasher passwordHasher,
            IAuditLogger auditLogger,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    _logger.LogWarning("Authentication failed: User {Username} not found", request.Username);
                    await _auditLogger.LogLoginAttemptAsync(request.Username, false, "N/A", DateTime.UtcNow);
                    throw new UserNotFoundException(request.Username);
                }

                if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for user {Username}", request.Username);
                    await _auditLogger.LogLoginAttemptAsync(request.Username, false, "N/A", DateTime.UtcNow);
                    throw new AuthenticationException("Invalid credentials");
                }

                user.LastLoginTime = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                await _auditLogger.LogLoginAttemptAsync(request.Username, true, "N/A", DateTime.UtcNow);

                var accessToken = _tokenGenerator.GenerateAccessToken(user);
                var refreshToken = _tokenGenerator.GenerateRefreshToken();

                return new AuthenticationResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = _tokenGenerator.TokenExpirationInMinutes * 60 // Convert to seconds
                };
            }
            catch (Exception ex) when (ex is not UserNotFoundException && ex is not AuthenticationException)
            {
                _logger.LogError(ex, "Error occurred during authentication for user {Username}", request.Username);
                throw;
            }
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var principal = _tokenGenerator.GetPrincipalFromToken(request.AccessToken);
                if (principal == null)
                {
                    throw new SecurityTokenException("Invalid access token");
                }

                var userId = int.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    throw new UserNotFoundException(userId);
                }

                var newAccessToken = _tokenGenerator.GenerateAccessToken(user);
                var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

                return new AuthenticationResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = _tokenGenerator.TokenExpirationInMinutes * 60 // Convert to seconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                throw;
            }
        }
    }
}
