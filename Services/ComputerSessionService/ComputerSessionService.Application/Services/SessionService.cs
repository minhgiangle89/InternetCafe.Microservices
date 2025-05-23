﻿using AutoMapper;
using ComputerSessionService.Application.DTOs.Computer;
using ComputerSessionService.Application.DTOs.Session;
using ComputerSessionService.Application.Interfaces;
using ComputerSessionService.Application.Interfaces.Repositories;
using ComputerSessionService.Application.Interfaces.Services;
using ComputerSessionService.Domain.Entities;
using ComputerSessionService.Domain.Exceptions;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComputerSessionService.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<SessionService> _logger;
        private readonly ITimeZoneService _timeZoneService;  
        public SessionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAccountServiceClient accountServiceClient,
            ICurrentUserService currentUserService,
            IAuditLogger auditLogger,
            ILogger<SessionService> logger,
            ITimeZoneService timeZoneService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accountServiceClient = accountServiceClient ?? throw new ArgumentNullException(nameof(accountServiceClient));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeZoneService = timeZoneService;
        }

        public async Task<SessionDTO> StartSessionAsync(StartSessionDTO startSessionDTO)
        {
            try
            {
                var now = _timeZoneService.GetVietnamTime();
                await _unitOfWork.BeginTransactionAsync();

                var existingUserSession = await HasActiveSessionAsync(startSessionDTO.UserId);
                if (existingUserSession)
                {
                    throw new Exception($"User with ID {startSessionDTO.UserId} already has an active session.");
                }

                var computer = await _unitOfWork.Computers.GetByIdAsync(startSessionDTO.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {startSessionDTO.ComputerId} not found.");
                }

                if (computer.ComputerStatus != (int)ComputerStatus.Available)
                {
                    throw new ComputerNotAvailableException(startSessionDTO.ComputerId);
                }

                var balance = await _accountServiceClient.GetAccountBalanceAsync(startSessionDTO.UserId);

                decimal minimumBalance = computer.HourlyRate / 4;
                if (balance < minimumBalance)
                {
                    throw new Exception($"Insufficient balance. Required: {minimumBalance}, Available: {balance}");
                }

                var session = new Session
                {
                    UserId = startSessionDTO.UserId,
                    ComputerId = startSessionDTO.ComputerId,
                    StartTime = now,
                    Duration = TimeSpan.Zero,
                    TotalCost = 0,
                    Status = (int)SessionStatus.Active
                };

                await _unitOfWork.Sessions.AddAsync(session);

                await _unitOfWork.Computers.UpdateStatusAsync(startSessionDTO.ComputerId, ComputerStatus.InUse);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditLogger.LogActivityAsync(
                    "SessionStarted",
                    "Session",
                    session.Id,
                    _currentUserService.UserId ?? startSessionDTO.UserId,
                    now,
                    $"Session started for user {startSessionDTO.UserId} on computer {startSessionDTO.ComputerId}");

                var sessionDto = _mapper.Map<SessionDTO>(session);
                sessionDto.ComputerName = computer.Name;

                return sessionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error starting session for user {UserId} on computer {ComputerId}",
                    startSessionDTO.UserId, startSessionDTO.ComputerId);
                throw;
            }
        }

        public async Task<SessionDTO> EndSessionAsync(EndSessionDTO endSessionDTO)
        {
            try
            {
                var now = _timeZoneService.GetVietnamTime();
                await _unitOfWork.BeginTransactionAsync();

                var session = await _unitOfWork.Sessions.GetByIdAsync(endSessionDTO.SessionId);
                if (session == null)
                {
                    throw new SessionNotFoundException(endSessionDTO.SessionId);
                }

                if (session.Status != (int)SessionStatus.Active)
                {
                    throw new Exception($"Session with ID {endSessionDTO.SessionId} is not active.");
                }

                var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {session.ComputerId} not found.");
                }

                var endTime = now;
                var duration = endTime - session.StartTime;
                var cost = CalculateSessionCost(duration, 1);

                session.EndTime = endTime;
                session.Duration = duration;
                session.TotalCost = cost;
                session.Status = (int)SessionStatus.Completed;
                session.Notes = endSessionDTO.Notes;

                await _unitOfWork.Sessions.UpdateAsync(session);

                await _unitOfWork.Computers.UpdateStatusAsync(session.ComputerId, ComputerStatus.Available);

                await _accountServiceClient.ChargeForSessionAsync(session.UserId, session.Id, cost);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log session end
                await _auditLogger.LogActivityAsync(
                    "SessionEnded",
                    "Session",
                    session.Id,
                    _currentUserService.UserId ?? session.UserId,
                    now,
                    $"Session ended for user {session.UserId}. Duration: {duration}, Cost: {cost}");

                var sessionDto = _mapper.Map<SessionDTO>(session);
                sessionDto.ComputerName = computer.Name;

                return sessionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error ending session {SessionId}", endSessionDTO.SessionId);
                throw;
            }
        }

        public async Task<SessionDTO> GetActiveSessionByComputerIdAsync(int computerId)
        {
            var session = await _unitOfWork.Sessions.GetCurrentSessionByComputerIdAsync(computerId);
            if (session == null)
            {
                return null;
            }

            var sessionDto = _mapper.Map<SessionDTO>(session);

            // Load related data
            var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
            sessionDto.ComputerName = computer?.Name ?? "Unknown";

            return sessionDto;
        }

        public async Task<IEnumerable<SessionDTO>> GetActiveSessionsAsync()
        {
            var sessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions).ToList();

            // Load related data
            foreach (var sessionDto in sessionDtos)
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(sessionDto.ComputerId);
                sessionDto.ComputerName = computer?.Name ?? "Unknown";
            }

            return sessionDtos;
        }

        public async Task<IEnumerable<SessionDTO>> GetSessionsByUserIdAsync(int userId)
        {
            var sessions = await _unitOfWork.Sessions.GetByUserIdAsync(userId);
            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions).ToList();

            // Load related data
            foreach (var sessionDto in sessionDtos)
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(sessionDto.ComputerId);
                sessionDto.ComputerName = computer?.Name ?? "Unknown";
            }

            return sessionDtos;
        }

        public async Task<IEnumerable<SessionDTO>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sessions = await _unitOfWork.Sessions.FindAsync(s =>
                s.StartTime >= startDate &&
                (s.EndTime == null || s.EndTime <= endDate));

            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions).ToList();

            // Load related data
            foreach (var sessionDto in sessionDtos)
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(sessionDto.ComputerId);
                sessionDto.ComputerName = computer?.Name ?? "Unknown";
            }

            return sessionDtos;
        }

        public async Task<decimal> CalculateSessionCostAsync(int sessionId)
        {
            var now = _timeZoneService.GetVietnamTime();
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                throw new SessionNotFoundException(sessionId);
            }

            if (session.EndTime == null)
            {
                // For active sessions, calculate current cost
                var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {session.ComputerId} not found.");
                }

                var currentDuration = now - session.StartTime;
                return CalculateSessionCost(currentDuration, computer.HourlyRate);
            }

            return session.TotalCost;
        }

        private decimal CalculateSessionCost(TimeSpan duration, decimal hourlyRate)
        {
            // Round up to the next minute
            double totalHours = Math.Ceiling(duration.TotalMinutes) / 60;
            return Math.Round(Convert.ToDecimal(totalHours) * hourlyRate, 2);
        }

        public async Task<TimeSpan> GetRemainingTimeAsync(int userId, int computerId)
        {
            var now = _timeZoneService.GetVietnamTime();
            var session = await _unitOfWork.Sessions.GetCurrentSessionByComputerIdAsync(computerId);
            if (session == null || session.UserId != userId)
            {
                throw new Exception("No active session found for this user and computer.");
            }

            var balance = await _accountServiceClient.GetAccountBalanceAsync(userId);

            var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
            if (computer == null)
            {
                throw new Exception($"Computer with ID {computerId} not found.");
            }
            var currentDuration = now - session.StartTime;
            var currentCost = CalculateSessionCost(currentDuration, computer.HourlyRate);
            var hourlyRate = computer.HourlyRate;

            if (hourlyRate <= 0)
            {
                return TimeSpan.FromDays(365);
            }

            var remainingHours = balance / hourlyRate;
            return TimeSpan.FromHours(Convert.ToDouble(remainingHours));
        }

        public async Task<bool> HasActiveSessionAsync(int userId)
        {
            var sessions = await _unitOfWork.Sessions.FindAsync(s =>
                s.UserId == userId && s.Status == (int)SessionStatus.Active);

            return sessions.Any();
        }

        public async Task<SessionDTO> TerminateSessionAsync(int sessionId, string reason)
        {
            try
            {
                var now = _timeZoneService.GetVietnamTime();
                await _unitOfWork.BeginTransactionAsync();

                var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
                if (session == null)
                {
                    throw new SessionNotFoundException(sessionId);
                }

                if (session.Status != (int)SessionStatus.Active)
                {
                    throw new Exception($"Session with ID {sessionId} is not active.");
                }

                var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {session.ComputerId} not found.");
                }

                var endTime = now;
                var duration = endTime - session.StartTime;
                var cost = CalculateSessionCost(duration, computer.HourlyRate);

                session.EndTime = endTime;
                session.Duration = duration;
                session.TotalCost = cost;
                session.Status = (int)SessionStatus.Terminated;
                session.Notes = reason;

                await _unitOfWork.Sessions.UpdateAsync(session);

                await _unitOfWork.Computers.UpdateStatusAsync(session.ComputerId, ComputerStatus.Available);

                await _accountServiceClient.ChargeForSessionAsync(session.UserId, session.Id, cost);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditLogger.LogActivityAsync(
                    "SessionTerminated",
                    "Session",
                    session.Id,
                    _currentUserService.UserId ?? session.UserId,
                    now,
                    $"Session terminated. Reason: {reason}. Duration: {duration}, Cost: {cost}");

                var sessionDto = _mapper.Map<SessionDTO>(session);
                sessionDto.ComputerName = computer.Name;

                return sessionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error terminating session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<SessionDetailsDTO> GetSessionDetailsAsync(int sessionId)
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                throw new SessionNotFoundException(sessionId);
            }

            var sessionDetailsDto = _mapper.Map<SessionDetailsDTO>(session);

            var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
            sessionDetailsDto.ComputerName = computer?.Name ?? "Unknown";

            return sessionDetailsDto;
        }

        public async Task<ComputerStatusSummaryDTO> GetComputerStatusSummaryAsync()
        {
            var allComputers = await _unitOfWork.Computers.GetAllAsync();
            var activeSessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var computerSessionMap = activeSessions.ToDictionary(s => s.ComputerId, s => s);

            var computerDetails = new List<ComputerStatusDetailDTO>();

            foreach (var computer in allComputers)
            {
                var detail = new ComputerStatusDetailDTO
                {
                    Id = computer.Id,
                    Name = computer.Name,
                    IpAddress = computer.IPAddress,
                    Location = computer.Location,
                    Status = (int)computer.ComputerStatus,
                    StatusText = GetStatusText(computer.ComputerStatus),
                    HourlyRate = computer.HourlyRate
                };

                if (computerSessionMap.TryGetValue(computer.Id, out var activeSession))
                {
                    detail.CurrentSessionId = activeSession.Id;
                    detail.CurrentUserId = activeSession.UserId;
                    detail.SessionStartTime = activeSession.StartTime;
                    detail.SessionDuration = CalculateDuration(activeSession.StartTime);
                    detail.CurrentSessionCost = await CalculateSessionCostAsync(activeSession.Id);
                }

                computerDetails.Add(detail);
            }
            var summary = new ComputerStatusSummaryDTO
            {
                TotalComputers = allComputers.Count(),
                AvailableComputers = computerDetails.Count(c => c.Status == (int)ComputerStatus.Available),
                ComputersInUse = computerDetails.Count(c => c.Status == (int)ComputerStatus.InUse),
                ComputersInMaintenance = computerDetails.Count(c => c.Status == (int)ComputerStatus.Maintenance),
                ComputersOutOfOrder = computerDetails.Count(c => c.Status == (int)ComputerStatus.OutOfOrder),
                ComputerDetails = computerDetails.OrderBy(c => c.Name).ToList()
            };

            return summary;
        }

        private string GetStatusText(int computerStatus)
        {
            switch (computerStatus)
            {
                case (int)ComputerStatus.Available:
                    return "Khả dụng";
                case (int)ComputerStatus.InUse:
                    return "Đang sử dụng";
                case (int)ComputerStatus.Maintenance:
                    return "Bảo trì";
                case (int)ComputerStatus.OutOfOrder:
                    return "Hỏng";
                default:
                    return "Không xác định";
            }
        }

        private string CalculateDuration(DateTime startTime)
        {
            var now = _timeZoneService.GetVietnamTime();
            var duration = now - startTime;
            return $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
        }
    }
}
