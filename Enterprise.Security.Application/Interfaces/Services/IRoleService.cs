using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>> GetAllRolesAsync(); // Cambiamos a DTO complejo para ver descripción
        Task<Result> CreateRoleAsync(CreateRoleDto dto);
        Task<Result> DeleteRoleAsync(string roleId);

        Task<Result> AssignRoleAsync(AssignRoleDto dto);
        Task<Result> UnassignRoleAsync(AssignRoleDto dto); // <--- NUEVO: Para quitar roles
        Task<Result<List<PermissionDto>>> GetPermissionsByRoleAsync(string roleId); // <-- Firma ajustada
        Task<Result> UpdatePermissionsAsync(UpdateRolePermissionsDto dto); // <--- NUEVO: Para actualizar permisos de rol
    }
}
