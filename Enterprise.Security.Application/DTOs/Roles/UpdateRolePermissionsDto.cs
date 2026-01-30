using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Roles
{
    // Usamos record para recibir la actualización
    public record UpdateRolePermissionsDto(
        string RoleId,
        List<string> PermissionIds
    );
}
