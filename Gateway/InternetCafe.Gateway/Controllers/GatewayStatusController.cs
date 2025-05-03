using Microsoft.AspNetCore.Mvc;

namespace InternetCafe.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayStatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Running",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
