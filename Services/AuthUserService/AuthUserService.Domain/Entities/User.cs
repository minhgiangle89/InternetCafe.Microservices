using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthUserService.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Role { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime Creation_Timestamp { get; set; }
        public int Creation_EmpId { get; set; }
        public DateTime LastUpdated_Timestamp { get; set; }
        public int LastUpdated_EmpId { get; set; }
        public int Status { get; set; }
    }
}
