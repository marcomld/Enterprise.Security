using Enterprise.Security.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Auditing
{
    public interface IAuditService
    {
        Task LogAsync(
            AuditAction action,
            string entity,
            Guid? userId,
            string ipAddress,
            string userAgent,
            string? entityId = null,
            string? additionalData = null);
    }
}
