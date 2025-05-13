using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.DTOs.Computer
{
    public class ComputerStatusDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public int Status { get; set; } // 0: Available, 1: InUse, 2: Maintenance, 3: OutOfOrder
        public string StatusText { get; set; }
        public decimal HourlyRate { get; set; }

        public int? CurrentSessionId { get; set; }
        public int? CurrentUserId { get; set; }
        public string? CurrentUserName { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public string? SessionDuration { get; set; }
        public decimal? CurrentSessionCost { get; set; }
    }
}
