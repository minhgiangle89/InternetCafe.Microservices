using AccountService.Application.DTOs.Account;
using AccountService.Application.DTOs.Extra;
using AccountService.Application.DTOs.Transaction;
using AccountService.Application.Interfaces.Services;
using InternetCafe.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Authorize(Roles = "2")] // Admin only
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        public async Task<ActionResult<ApiResponse<AccountDTO>>> CreateAccount([FromBody] int userId)
        {
            try
            {
                var account = await _accountService.CreateAccountAsync(userId);
                return CreatedAtAction(nameof(GetAccountById), new { id = account.Id },
                    ApiResponseFactory.Success(account, "Tài khoản được tạo thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tài khoản cho người dùng {UserId}", userId);
                return BadRequest(ApiResponseFactory.Fail<AccountDTO>($"Tạo tài khoản không thành công: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<AccountDTO>>> GetAccountById(int id)
        {
            try
            {
                var account = await _accountService.GetAccountByUserIdAsync(id);
                return Ok(ApiResponseFactory.Success(account, "Thông tin tài khoản được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy tài khoản có ID {AccountId}", id);
                return NotFound(ApiResponseFactory.Fail<AccountDTO>($"Không tìm thấy tài khoản có ID {id}"));
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<AccountDTO>>> GetAccountByUserId(int userId)
        {
            try
            {
                var account = await _accountService.GetAccountByUserIdAsync(userId);
                return Ok(ApiResponseFactory.Success(account, "Thông tin tài khoản được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy tài khoản cho người dùng có ID {UserId}", userId);
                return NotFound(ApiResponseFactory.Fail<AccountDTO>($"Không tìm thấy tài khoản cho người dùng có ID {userId}"));
            }
        }

        [HttpGet("user/{userId}/balance")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetBalanceByUserId(int userId)
        {
            try
            {
                var balance = await _accountService.GetBalanceByUserIdAsync(userId);
                return Ok(ApiResponseFactory.Success(balance, "Số dư tài khoản được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy tài khoản cho người dùng có ID {UserId}", userId);
                return NotFound(ApiResponseFactory.Fail<decimal>($"Không tìm thấy tài khoản cho người dùng có ID {userId}"));
            }
        }

        [HttpGet("{id}/transactions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDTO>>>> GetTransactionsByAccountId(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var transactions = await _accountService.GetTransactionsByAccountIdAsync(id, pageNumber, pageSize);
                return Ok(ApiResponseFactory.Success(transactions, "Danh sách giao dịch được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy tài khoản có ID {AccountId}", id);
                return NotFound(ApiResponseFactory.Fail<IEnumerable<TransactionDTO>>($"Không tìm thấy tài khoản có ID {id}"));
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<AccountDetailsDTO>>> GetAccountDetails(int id)
        {
            try
            {
                var accountDetails = await _accountService.GetAccountWithTransactionsAsync(id);
                return Ok(ApiResponseFactory.Success(accountDetails, "Chi tiết tài khoản được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy tài khoản có ID {AccountId}", id);
                return NotFound(ApiResponseFactory.Fail<AccountDetailsDTO>($"Không tìm thấy tài khoản có ID {id}"));
            }
        }

        [HttpPost("deposit")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<TransactionDTO>>> Deposit([FromBody] DepositDTO depositDTO)
        {
            try
            {
                var transaction = await _accountService.DepositAsync(depositDTO);
                return Ok(ApiResponseFactory.Success(transaction, "Nạp tiền thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nạp tiền vào tài khoản {AccountId}", depositDTO.AccountId);
                return BadRequest(ApiResponseFactory.Fail<TransactionDTO>($"Nạp tiền không thành công: {ex.Message}"));
            }
        }

        [HttpPost("withdraw")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<TransactionDTO>>> Withdraw([FromBody] WithdrawDTO withdrawDTO)
        {
            try
            {
                var transaction = await _accountService.WithdrawAsync(withdrawDTO);
                return Ok(ApiResponseFactory.Success(transaction, "Rút tiền thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi rút tiền từ tài khoản {AccountId}", withdrawDTO.AccountId);
                return BadRequest(ApiResponseFactory.Fail<TransactionDTO>($"Rút tiền không thành công: {ex.Message}"));
            }
        }

        [HttpPost("charge")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<TransactionDTO>>> ChargeForSession([FromBody] SessionChargeDTO chargeDTO)
        {
            try
            {
                var transaction = await _accountService.ChargeForSessionAsync(
                    chargeDTO.AccountId, chargeDTO.SessionId, chargeDTO.Amount);
                return Ok(ApiResponseFactory.Success(transaction, "Thanh toán phiên sử dụng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thanh toán phiên {SessionId} cho tài khoản {AccountId}",
                    chargeDTO.SessionId, chargeDTO.AccountId);
                return BadRequest(ApiResponseFactory.Fail<TransactionDTO>($"Thanh toán không thành công: {ex.Message}"));
            }
        }

        [HttpGet("{id}/has-sufficient-balance")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        public async Task<ActionResult<ApiResponse<bool>>> HasSufficientBalance(int id, [FromQuery] decimal amount)
        {
            try
            {
                var hasSufficientBalance = await _accountService.HasSufficientBalanceAsync(id, amount);
                return Ok(ApiResponseFactory.Success(hasSufficientBalance,
                    hasSufficientBalance ? "Số dư đủ" : "Số dư không đủ"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy tài khoản có ID {AccountId}", id);
                return NotFound(ApiResponseFactory.Fail<bool>($"Không tìm thấy tài khoản có ID {id}"));
            }
        }
    }
}
