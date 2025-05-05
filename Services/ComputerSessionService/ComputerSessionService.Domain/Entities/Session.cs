using InternetCafe.Common.Entities;
using InternetCafe.Common.Enums;

namespace ComputerSessionService.Domain.Entities
{
    public class Session : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ComputerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal TotalCost { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Active;
        public string? Notes { get; set; }

        public Computer Computer { get; set; } = null!;
    }
}
