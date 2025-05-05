using ComputerSessionService.Application.DTOs.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.DTOs.Computer
{
    public class ComputerDetailsDTO : ComputerDTO
    {
        public SessionDTO? CurrentSession { get; set; }
        public ICollection<SessionSummaryDTO> RecentSessions { get; set; } = new List<SessionSummaryDTO>();
    }
}
