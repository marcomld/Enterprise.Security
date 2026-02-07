using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Audit
{
    public record AuditLogResponseDto(
     Guid Id,            //CAMBIO: De int a Guid
     Guid? UserId,     //CAMBIO: Puede ser nulo (string?)
     string UserEmail,   //CAMBIO: Agregado para mostrar el email del usuario
     string Action,
     string Entity,
     string? EntityId,   // Puede ser nulo
     string? IpAddress,  // Puede ser nulo
     string? AdditionalData, // Puede ser nulo
     DateTime CreatedAt
 );
}
