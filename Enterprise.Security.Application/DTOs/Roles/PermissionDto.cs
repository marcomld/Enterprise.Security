using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Roles
{
    // Usamos record posicional para enviar datos al cliente
    public record PermissionDto(
        string PermissionId,
        string Module,
        string Code,
        string Description,
        bool IsSelected // True si el rol ya tiene este permiso
    );
}
