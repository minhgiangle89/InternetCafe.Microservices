using AuthUserService.Application.DTOs.Authentication;

namespace AuthUserService.Application.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}
