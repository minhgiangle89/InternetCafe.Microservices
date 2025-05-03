using InternetCafe.Common.Exceptions;

namespace AuthUserService.Domain.Exceptions
{
    public class UserNotFoundException : DomainException
    {
        public UserNotFoundException(int userId) : base($"User with ID {userId} was not found.")
        {
            UserId = userId;
        }

        public UserNotFoundException(string username) : base($"User with username '{username}' was not found.")
        {
            Username = username;
        }

        public int? UserId { get; }
        public string? Username { get; }
    }
}
