using AccountService.Application.DTOs.Account;
using AccountService.Application.DTOs.Transaction;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Services;
using AccountService.Domain.Entities;
using AutoMapper;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Interfaces;
using Microsoft.Extensions.Logging;


namespace AccountService.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IAuditLogger auditLogger,
            ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<AccountDTO>> GetAllAccountAsync()
        {
            var accounts = await _unitOfWork.Accounts.GetAllAsync();
            return _mapper.Map<IEnumerable<AccountDTO>>(accounts);
        }

        public async Task<AccountDTO> CreateAccountAsync(int userId)
        {
            try
            {
                var existingAccount = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
                if (existingAccount != null)
                {
                    _logger.LogWarning("Account already exists for user {UserId}", userId);
                    return _mapper.Map<AccountDTO>(existingAccount);
                }

                var account = new Account
                {
                    UserId = userId,
                    Balance = 0,
                    LastDepositDate = DateTime.UtcNow,
                    LastUsageDate = DateTime.UtcNow
                };

                await _unitOfWork.Accounts.AddAsync(account);
                await _unitOfWork.CompleteAsync();

                await _auditLogger.LogActivityAsync(
                    "AccountCreated",
                    "Account",
                    account.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Account created for user {userId}");

                var accountDto = _mapper.Map<AccountDTO>(account);
                return accountDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account for user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetBalanceAsync(int accountId)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            return account.Balance;
        }

        public async Task<decimal> GetBalanceByUserIdAsync(int userId)
        {
            var account = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new Exception($"Account for user with ID {userId} not found.");
            }

            return account.Balance;
        }

        public async Task<TransactionDTO> DepositAsync(DepositDTO depositDTO)
        {
            try
            {
                if (depositDTO.Amount <= 0)
                {
                    throw new ArgumentException("Deposit amount must be greater than zero.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var account = await _unitOfWork.Accounts.GetByIdAsync(depositDTO.AccountId);
                if (account == null)
                {
                    throw new Exception($"Account with ID {depositDTO.AccountId} not found.");
                }

                account.Balance += depositDTO.Amount;
                account.LastDepositDate = DateTime.UtcNow;
                await _unitOfWork.Accounts.UpdateAsync(account);

                var transaction = new Transaction
                {
                    AccountId = depositDTO.AccountId,
                    Amount = depositDTO.Amount,
                    Type = TransactionType.Deposit,
                    PaymentMethod = (PaymentMethod)depositDTO.PaymentMethod,
                    ReferenceNumber = depositDTO.ReferenceNumber,
                    Description = "Deposit to account",
                    UserId = _currentUserService.UserId
                };

                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditLogger.LogActivityAsync(
                    "AccountDeposit",
                    "Account",
                    account.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Deposit of {depositDTO.Amount} to account {account.Id}");

                var transactionDto = _mapper.Map<TransactionDTO>(transaction);
                return transactionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error depositing to account {AccountId}", depositDTO.AccountId);
                throw;
            }
        }

        public async Task<TransactionDTO> WithdrawAsync(WithdrawDTO withdrawDTO)
        {
            try
            {
                if (withdrawDTO.Amount <= 0)
                {
                    throw new ArgumentException("Withdrawal amount must be greater than zero.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var account = await _unitOfWork.Accounts.GetByIdAsync(withdrawDTO.AccountId);
                if (account == null)
                {
                    throw new Exception($"Account with ID {withdrawDTO.AccountId} not found.");
                }

                if (account.Balance < withdrawDTO.Amount)
                {
                    throw new Exception($"Insufficient balance. Current: {account.Balance}, Required: {withdrawDTO.Amount}");
                }

                account.Balance -= withdrawDTO.Amount;
                account.LastUsageDate = DateTime.UtcNow;
                await _unitOfWork.Accounts.UpdateAsync(account);

                var transaction = new Transaction
                {
                    AccountId = withdrawDTO.AccountId,
                    Amount = -withdrawDTO.Amount, // Negative amount for withdrawal
                    Type = TransactionType.Withdrawal,
                    Description = withdrawDTO.Reason ?? "Withdrawal from account",
                    UserId = _currentUserService.UserId
                };

                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditLogger.LogActivityAsync(
                    "AccountWithdrawal",
                    "Account",
                    account.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Withdrawal of {withdrawDTO.Amount} from account {account.Id}");

                var transactionDto = _mapper.Map<TransactionDTO>(transaction);
                return transactionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error withdrawing from account {AccountId}", withdrawDTO.AccountId);
                throw;
            }
        }

        public async Task<TransactionDTO> ChargeForSessionAsync(int accountId, int sessionId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    throw new ArgumentException("Charge amount must be greater than zero.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null)
                {
                    throw new Exception($"Account with ID {accountId} not found.");
                }

                if (account.Balance < amount)
                {
                    throw new Exception($"Insufficient balance. Current: {account.Balance}, Required: {amount}");
                }

                account.Balance -= amount;
                account.LastUsageDate = DateTime.UtcNow;
                await _unitOfWork.Accounts.UpdateAsync(account);

                var transaction = new Transaction
                {
                    AccountId = accountId,
                    Amount = -amount, // Negative amount for charge
                    Type = TransactionType.ComputerUsage,
                    Description = $"Charge for session #{sessionId}",
                    SessionId = sessionId
                };

                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditLogger.LogActivityAsync(
                    "SessionCharge",
                    "Account",
                    account.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Charge of {amount} for session {sessionId}");

                var transactionDto = _mapper.Map<TransactionDTO>(transaction);
                return transactionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error charging account {AccountId} for session {SessionId}", accountId, sessionId);
                throw;
            }
        }

        public async Task<bool> HasSufficientBalanceAsync(int accountId, decimal amount)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            return account.Balance >= amount;
        }

        public async Task<AccountDetailsDTO> GetAccountWithTransactionsAsync(int accountId)
        {
            var account = await _unitOfWork.Accounts.GetWithTransactionsAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            var result = _mapper.Map<AccountDetailsDTO>(account);

            var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(accountId);
            result.RecentTransactions = _mapper.Map<ICollection<TransactionDTO>>(
                transactions.Take(10).ToList());

            return result;
        }

        public async Task<AccountDTO> GetAccountByUserIdAsync(int userId)
        {
            var account = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new Exception($"Account for user with ID {userId} not found.");
            }

            var result = _mapper.Map<AccountDTO>(account);
            return result;
        }

        public async Task<IEnumerable<TransactionDTO>> GetTransactionsByAccountIdAsync(int accountId, int pageNumber, int pageSize)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(accountId);

            var paginatedTransactions = transactions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = _mapper.Map<IEnumerable<TransactionDTO>>(paginatedTransactions);
            return result;
        }
    }
}
