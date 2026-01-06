using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Audit
{
    public record AuditLogResponseDto(
    int Id,
    string UserId,
    string Action,      // Ej: "UserCreated"
    string Entity,      // Ej: "User"
    string EntityId,
    string IpAddress,
    string AdditionalData,
    DateTime CreatedAt
    );
}
