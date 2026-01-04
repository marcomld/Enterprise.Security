using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Roles
{
    public record AssignRoleDto(Guid UserId, string RoleName);
}
