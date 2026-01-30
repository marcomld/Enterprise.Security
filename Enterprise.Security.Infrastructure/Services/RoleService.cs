using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Roles;
using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Domain.Enums;
using Enterprise.Security.Infrastructure.Identity;
using Enterprise.Security.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SecurityDbContext _context; // 👈 1. Declaramos la variable
        private readonly IAuditService _audit;


        public RoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, // <--- Usa tu clase personalizada
            SecurityDbContext context, // <--- ¡Esto faltaba!
            IAuditService audit)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context; // <--- ¡Y esto!
            _audit = audit;
        }

        // 1. Listar con detalles
        public async Task<List<RoleResponseDto>> GetAllRolesAsync()
        {
            return await _roleManager.Roles
                .Select(r => new RoleResponseDto(r.Id.ToString(), r.Name!, r.Description))
                .ToListAsync();
        }

        // 2. Crear Rol
        public async Task<Result> CreateRoleAsync(CreateRoleDto dto)
        {
            if (await _roleManager.RoleExistsAsync(dto.RoleName))
                return Result.Failure("El rol ya existe.");

            var role = new ApplicationRole(dto.RoleName)
            {
                Description = dto.Description,
                IsSystemRole = false // Los creados por UI no son de sistema
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                await _audit.LogAsync(AuditAction.RoleCreated, "Role", null, "N/A", "API", $"Role: {dto.RoleName}");
                return Result.Success();
            }

            return Result.Failure(result.Errors.First().Description);
        }

        // 3. Borrar Rol
        public async Task<Result> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return Result.Failure("Rol no encontrado");

            if (role.IsSystemRole) return Result.Failure("No se pueden borrar roles de sistema.");

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                await _audit.LogAsync(AuditAction.RoleDeleted, "Role", null, "N/A", "API", $"Deleted Role: {role.Name}");
                return Result.Success();
            }

            return Result.Failure("Error al eliminar rol");
        }

        // 4. Asignar (Ya lo tenías, asegúrate que use el DTO correcto)
        public async Task<Result> AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null) return Result.Failure("Usuario no encontrado");

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
                return Result.Failure("Rol no existe");

            // Identity se encarga de la tabla AspNetUserRoles
            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);

            if (!result.Succeeded)
                return Result.Failure(result.Errors.First().Description);

            await _audit.LogAsync(
                action: AuditAction.RoleAssignedToUser,
                entity: "User",
                userId: dto.UserId,
                ipAddress: "N/A",
                userAgent: "API",
                additionalData: $"Role: {dto.RoleName}");

            return Result.Success();
        }

        // 5. Desasignar (Quitar Rol)
        public async Task<Result> UnassignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null) return Result.Failure("Usuario no encontrado");

            var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);

            if (!result.Succeeded)
                return Result.Failure("Error al remover rol");

            await _audit.LogAsync(
                action: AuditAction.RoleRemovedFromUser, // Asegúrate de tener este Enum o usa uno genérico
                entity: "User",
                userId: dto.UserId,
                ipAddress: "N/A",
                userAgent: "API",
                additionalData: $"Removed Role: {dto.RoleName}");

            return Result.Success();
        }

        // --- MÉTODOS DE PERMISOS (CORREGIDOS) ---

        public async Task<Result<List<PermissionDto>>> GetPermissionsByRoleAsync(string roleId)
        {
            // Convertimos string a Guid para buscar (si tu FindById espera string, está bien, si no, Guid.Parse)
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return Result<List<PermissionDto>>.Failure("Rol no encontrado");

            // Convertimos el RoleId a Guid para usarlo en la consulta LINQ
            if (!Guid.TryParse(roleId, out var roleGuid))
                return Result<List<PermissionDto>>.Failure("ID de rol inválido");

            var allPermissions = await _context.Permissions.ToListAsync();

            var assignedPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleGuid) // 👈 Usamos el Guid parseado
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var result = allPermissions.Select(p => new PermissionDto(
                p.Id.ToString(), // Convertimos el Guid a String para el DTO
                p.Module,
                p.Code,
                p.Description,
                assignedPermissionIds.Contains(p.Id)
            )).ToList();

            return Result<List<PermissionDto>>.Success(result);
        }

        public async Task<Result> UpdatePermissionsAsync(UpdateRolePermissionsDto request)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null) return Result.Failure("Rol no encontrado");

            if (role.Name == "Admin")
                return Result.Failure("No se pueden modificar los permisos del Administrador Global.");

            if (!Guid.TryParse(request.RoleId, out var roleGuid))
                return Result.Failure("ID de rol inválido");

            // 1. Borramos existentes
            var currentPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleGuid)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(currentPermissions);

            // 2. Insertamos nuevos (CORRECCIÓN DE GUID) 👈
            foreach (var permissionIdString in request.PermissionIds)
            {
                if (Guid.TryParse(permissionIdString, out var permissionGuid))
                {
                    // El constructor de RolePermission espera (Guid, Guid)
                    _context.RolePermissions.Add(new RolePermission(roleGuid, permissionGuid));
                }
            }

            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                AuditAction.RoleUpdated,
                "Role",
                roleGuid, // Pasamos el Guid
                "N/A",
                "API",
                $"Permisos actualizados. Total: {request.PermissionIds.Count}"
            );

            return Result.Success();
        }
    }
}
