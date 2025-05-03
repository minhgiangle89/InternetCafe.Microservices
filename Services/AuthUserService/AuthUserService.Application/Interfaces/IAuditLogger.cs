using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthUserService.Application.Interfaces
{
    public interface IAuditLogger
    {
        Task LogActivityAsync(
            string action,
            string entityName,
            int entityId,
            int userId,
            DateTime timestamp,
            string? details = null);

        Task LogLoginAttemptAsync(
            string username,
            bool success,
            string ipAddress,
            DateTime timestamp);

        Task LogSystemEventAsync(
            string eventType,
            string description,
            DateTime timestamp,
            int? userId = null);
    }
}
