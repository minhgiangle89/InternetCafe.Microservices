using ComputerSessionService.Application.DTOs.Session;
using ComputerSessionService.Application.Interfaces.Services;
using ComputerSessionService.Domain.Exceptions;
using InternetCafe.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerSessionService.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<SessionController> _logger;

        public SessionController(
            ISessionService sessionService,
            ILogger<SessionController> logger)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("start")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> StartSession([FromBody] StartSessionDTO startSessionDTO)
        {
            try
            {
                var session = await _sessionService.StartSessionAsync(startSessionDTO);
                return CreatedAtAction(nameof(GetSessionById), new { id = session.Id },
                    ApiResponseFactory.Success(session, "Bắt đầu phiên sử dụng thành công"));
            }
            catch (ComputerNotAvailableException ex)
            {
                _logger.LogWarning(ex, "Máy tính {ComputerId} không khả dụng để bắt đầu phiên", startSessionDTO.ComputerId);
                return BadRequest(ApiResponseFactory.Fail<SessionDTO>($"Máy tính không khả dụng: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bắt đầu phiên sử dụng cho người dùng {UserId} trên máy tính {ComputerId}",
                    startSessionDTO.UserId, startSessionDTO.ComputerId);
                return BadRequest(ApiResponseFactory.Fail<SessionDTO>($"Bắt đầu phiên sử dụng không thành công: {ex.Message}"));
            }
        }

        [HttpPost("end")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> EndSession([FromBody] EndSessionDTO endSessionDTO)
        {
            try
            {
                var session = await _sessionService.EndSessionAsync(endSessionDTO);
                return Ok(ApiResponseFactory.Success(session, "Kết thúc phiên sử dụng thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên sử dụng có ID {SessionId}", endSessionDTO.SessionId);
                return NotFound(ApiResponseFactory.Fail<SessionDTO>($"Không tìm thấy phiên sử dụng: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kết thúc phiên sử dụng {SessionId}", endSessionDTO.SessionId);
                return BadRequest(ApiResponseFactory.Fail<SessionDTO>($"Kết thúc phiên sử dụng không thành công: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<SessionDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDetailsDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDetailsDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<SessionDetailsDTO>>> GetSessionById(int id)
        {
            try
            {
                var session = await _sessionService.GetSessionDetailsAsync(id);
                return Ok(ApiResponseFactory.Success(session, "Thông tin phiên sử dụng được tải thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên sử dụng có ID {SessionId}", id);
                return NotFound(ApiResponseFactory.Fail<SessionDetailsDTO>($"Không tìm thấy phiên sử dụng có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phiên sử dụng có ID {SessionId}", id);
                return StatusCode(500, ApiResponseFactory.Fail<SessionDetailsDTO>("Lỗi server khi lấy thông tin phiên sử dụng"));
            }
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SessionDTO>>>> GetActiveSessions()
        {
            try
            {
                var sessions = await _sessionService.GetActiveSessionsAsync();
                return Ok(ApiResponseFactory.Success(sessions, "Danh sách phiên sử dụng đang hoạt động được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiên sử dụng đang hoạt động");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Lỗi server khi lấy danh sách phiên sử dụng đang hoạt động"));
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SessionDTO>>>> GetSessionsByUserId(int userId)
        {
            try
            {
                var sessions = await _sessionService.GetSessionsByUserIdAsync(userId);
                return Ok(ApiResponseFactory.Success(sessions, "Danh sách phiên sử dụng của người dùng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiên sử dụng của người dùng {UserId}", userId);
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Lỗi server khi lấy danh sách phiên sử dụng của người dùng"));
            }
        }

        [HttpGet("computer/{computerId}/active")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> GetActiveSessionByComputerId(int computerId)
        {
            try
            {
                var session = await _sessionService.GetActiveSessionByComputerIdAsync(computerId);
                if (session == null)
                {
                    return NotFound(ApiResponseFactory.Fail<SessionDTO>($"Không tìm thấy phiên sử dụng đang hoạt động cho máy tính có ID {computerId}"));
                }
                return Ok(ApiResponseFactory.Success(session, "Thông tin phiên sử dụng đang hoạt động được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phiên sử dụng đang hoạt động cho máy tính {ComputerId}", computerId);
                return StatusCode(500, ApiResponseFactory.Fail<SessionDTO>("Lỗi server khi lấy thông tin phiên sử dụng đang hoạt động"));
            }
        }

        [HttpGet("date-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SessionDTO>>>> GetSessionsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Ngày bắt đầu không thể sau ngày kết thúc"));
                }

                var sessions = await _sessionService.GetSessionsByDateRangeAsync(startDate, endDate);
                return Ok(ApiResponseFactory.Success(sessions, "Danh sách phiên sử dụng theo khoảng thời gian được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiên sử dụng theo khoảng thời gian từ {StartDate} đến {EndDate}", startDate, endDate);
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Lỗi server khi lấy danh sách phiên sử dụng theo khoảng thời gian"));
            }
        }

        [HttpGet("{id}/cost")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 401)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<decimal>>> CalculateSessionCost(int id)
        {
            try
            {
                var cost = await _sessionService.CalculateSessionCostAsync(id);
                return Ok(ApiResponseFactory.Success(cost, "Chi phí phiên sử dụng được tính toán thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên sử dụng có ID {SessionId}", id);
                return NotFound(ApiResponseFactory.Fail<decimal>($"Không tìm thấy phiên sử dụng có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính toán chi phí phiên sử dụng có ID {SessionId}", id);
                return StatusCode(500, ApiResponseFactory.Fail<decimal>("Lỗi server khi tính toán chi phí phiên sử dụng"));
            }
        }

        [HttpGet("user/{userId}/computer/{computerId}/remaining-time")]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 401)]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<TimeSpan>>> GetRemainingTime(int userId, int computerId)
        {
            try
            {
                var remainingTime = await _sessionService.GetRemainingTimeAsync(userId, computerId);
                return Ok(ApiResponseFactory.Success(remainingTime, "Thời gian còn lại được tính toán thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính toán thời gian còn lại cho người dùng {UserId} trên máy tính {ComputerId}", userId, computerId);
                return BadRequest(ApiResponseFactory.Fail<TimeSpan>($"Lỗi khi tính toán thời gian còn lại: {ex.Message}"));
            }
        }

        [HttpPost("{id}/terminate")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> TerminateSession(int id, [FromBody] string reason)
        {
            try
            {
                var session = await _sessionService.TerminateSessionAsync(id, reason);
                return Ok(ApiResponseFactory.Success(session, "Chấm dứt phiên sử dụng thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên sử dụng có ID {SessionId}", id);
                return NotFound(ApiResponseFactory.Fail<SessionDTO>($"Không tìm thấy phiên sử dụng có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấm dứt phiên sử dụng có ID {SessionId}", id);
                return BadRequest(ApiResponseFactory.Fail<SessionDTO>($"Chấm dứt phiên sử dụng không thành công: {ex.Message}"));
            }
        }

        [HttpGet("user/{userId}/has-active")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> HasActiveSession(int userId)
        {
            try
            {
                var hasActiveSession = await _sessionService.HasActiveSessionAsync(userId);
                return Ok(ApiResponseFactory.Success(hasActiveSession, "Kiểm tra phiên sử dụng đang hoạt động thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra phiên sử dụng đang hoạt động cho người dùng {UserId}", userId);
                return StatusCode(500, ApiResponseFactory.Fail<bool>("Lỗi server khi kiểm tra phiên sử dụng đang hoạt động"));
            }
        }
    }
}

