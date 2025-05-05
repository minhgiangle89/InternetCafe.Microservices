using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.DTOs.Session
{
    public class StartSessionDTO
    {
        public int UserId { get; set; }
        public int ComputerId { get; set; }
    }
}
