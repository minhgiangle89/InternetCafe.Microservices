using InternetCafe.Common.Entities;

namespace ComputerSessionService.Domain.Entities
{
    public class Computer : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ComputerStatus { get; set; } = 1;
        public decimal HourlyRate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
