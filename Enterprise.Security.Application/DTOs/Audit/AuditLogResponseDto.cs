using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Audit
{
    public record AuditLogResponseDto(
        Guid Id,
        Guid? UserId,
        string? UserEmail,
        string Action,
        string Entity,
        string? EntityId,
        string IpAddress,
        string UserAgent, // <--- NUEVO CAMPO AGREGADO
        string? AdditionalData,
        DateTime CreatedAt
    );
}
