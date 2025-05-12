using ComputerSessionService.Application.DTOs.Computer;
using ComputerSessionService.Application.Interfaces.Services;
using InternetCafe.Common.Api;
using InternetCafe.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerSessionService.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComputerController : ControllerBase
    {
        private readonly IComputerService _computerService;
        private readonly ILogger<ComputerController> _logger;

        public ComputerController(
            IComputerService computerService,
            ILogger<ComputerController> logger)
        {
            _computerService = computerService ?? throw new ArgumentNullException(nameof(computerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComputerDTO>>>> GetAllComputers()
        {
            try
            {
                var computers = await _computerService.GetAllComputersAsync();
                return Ok(ApiResponseFactory.Success(computers, "Danh sách máy tính được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách máy tính");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Lỗi server khi lấy danh sách máy tính"));
            }
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComputerDTO>>>> GetAvailableComputers()
        {
            try
            {
                var computers = await _computerService.GetAvailableComputersAsync();
                return Ok(ApiResponseFactory.Success(computers, "Danh sách máy tính khả dụng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách máy tính khả dụng");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Lỗi server khi lấy danh sách máy tính khả dụng"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDTO>>> GetComputerById(int id)
        {
            try
            {
                var computer = await _computerService.GetComputerByIdAsync(id);
                return Ok(ApiResponseFactory.Success(computer, "Thông tin máy tính được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy máy tính có ID {ComputerId}", id);
                return NotFound(ApiResponseFactory.Fail<ComputerDTO>($"Không tìm thấy máy tính có ID {id}"));
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<ComputerDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDetailsDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDetailsDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDetailsDTO>>> GetComputerDetails(int id)
        {
            try
            {
                var computerDetails = await _computerService.GetComputerDetailsAsync(id);
                return Ok(ApiResponseFactory.Success(computerDetails, "Chi tiết máy tính được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy chi tiết máy tính có ID {ComputerId}", id);
                return NotFound(ApiResponseFactory.Fail<ComputerDetailsDTO>($"Không tìm thấy chi tiết máy tính có ID {id}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "2")] // Admin only
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDTO>>> RegisterComputer([FromBody] CreateComputerDTO computerDTO)
        {
            try
            {
                var computer = await _computerService.RegisterComputerAsync(computerDTO);
                return CreatedAtAction(nameof(GetComputerById), new { id = computer.Id },
                    ApiResponseFactory.Success(computer, "Đăng ký máy tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký máy tính {ComputerName}", computerDTO.Name);
                return BadRequest(ApiResponseFactory.Fail<ComputerDTO>($"Đăng ký máy tính không thành công: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "2")] // Admin only
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDTO>>> UpdateComputer(int id, [FromBody] UpdateComputerDTO computerDTO)
        {
            try
            {
                var computer = await _computerService.UpdateComputerAsync(id, computerDTO);
                return Ok(ApiResponseFactory.Success(computer, "Cập nhật máy tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật máy tính có ID {ComputerId}", id);
                return BadRequest(ApiResponseFactory.Fail<ComputerDTO>($"Cập nhật máy tính không thành công: {ex.Message}"));
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "2")]
        [ProducesResponseType(typeof(ApiResponseBase), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponseBase>> SetComputerStatus(int id, [FromBody] ComputerStatusUpdateDTO updateDTO)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ComputerStatus), updateDTO.Status))
                {
                    return BadRequest(ApiResponseFactory.Fail("Giá trị trạng thái không hợp lệ"));
                }

                updateDTO.ComputerId = id; // Ensure the ID in the route is used
                await _computerService.SetComputerStatusAsync(updateDTO);
                return Ok(ApiResponseFactory.Success("Cập nhật trạng thái máy tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái máy tính có ID {ComputerId}", id);
                return BadRequest(ApiResponseFactory.Fail($"Cập nhật trạng thái máy tính không thành công: {ex.Message}"));
            }
        }

        [HttpPut("{id}/maintenance")]
        [Authorize(Roles = "2")]
        [ProducesResponseType(typeof(ApiResponseBase), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponseBase>> SetComputerMaintenance(int id, [FromBody] string reason)
        {
            try
            {
                await _computerService.SetComputerMaintenanceAsync(id, reason);
                return Ok(ApiResponseFactory.Success("Đặt máy tính vào chế độ bảo trì thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt máy tính vào chế độ bảo trì có ID {ComputerId}", id);
                return BadRequest(ApiResponseFactory.Fail($"Đặt máy tính vào chế độ bảo trì không thành công: {ex.Message}"));
            }
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComputerDTO>>>> GetComputersByStatus(int status)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ComputerStatus), status))
                {
                    return BadRequest(ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Giá trị trạng thái không hợp lệ"));
                }

                var computers = await _computerService.GetComputersByStatusAsync((ComputerStatus)status);
                return Ok(ApiResponseFactory.Success(computers, $"Danh sách máy tính có trạng thái {(ComputerStatus)status} được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách máy tính theo trạng thái {Status}", status);
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Lỗi server khi lấy danh sách máy tính theo trạng thái"));
            }
        }
    }
}
