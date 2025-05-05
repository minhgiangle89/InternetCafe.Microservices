using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.DTOs.Session
{
    public class EndSessionDTO
    {
        public int SessionId { get; set; }
        public string? Notes { get; set; }
    }
}
