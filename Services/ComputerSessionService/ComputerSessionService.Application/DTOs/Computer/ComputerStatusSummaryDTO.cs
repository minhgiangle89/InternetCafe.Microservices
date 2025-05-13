using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.DTOs.Computer
{
    public class ComputerStatusSummaryDTO
    {
        public int TotalComputers { get; set; }
        public int AvailableComputers { get; set; }
        public int ComputersInUse { get; set; }
        public int ComputersInMaintenance { get; set; }
        public int ComputersOutOfOrder { get; set; }
        public List<ComputerStatusDetailDTO> ComputerDetails { get; set; } = new List<ComputerStatusDetailDTO>();
    }
}
