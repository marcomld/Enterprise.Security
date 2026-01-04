using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Permissions;
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
    public class PermissionService : IPermissionService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SecurityDbContext _context; // Necesitamos acceso directo a tablas personalizadas
        private readonly IAuditService _audit;

        public PermissionService(
            RoleManager<ApplicationRole> roleManager,
            SecurityDbContext context,
            IAuditService audit)
        {
            _roleManager = roleManager;
            _context = context;
            _audit = audit;
        }

        public async Task<List<string>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.Select(p => p.Code).ToListAsync();
        }

        public async Task<Result> AssignPermissionToRoleAsync(AssignPermissionDto dto)
        {
            // 1. Validar Rol
            var role = await _roleManager.FindByNameAsync(dto.RoleName);
            if (role == null) return Result.Failure("Rol no encontrado");

            // 2. Validar Permiso (en tabla Permissions del Dominio)
            var permissionEntity = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == dto.Permission);

            if (permissionEntity == null) return Result.Failure("Permiso no definido en el sistema");

            // 3. Verificar si ya existe la asignación
            var exists = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permissionEntity.Id);

            if (exists) return Result.Failure("El rol ya tiene este permiso");

            // 4. Crear relación en tabla personalizada RolePermissions
            var rolePermission = new RolePermission(role.Id, permissionEntity.Id);

            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.SaveChangesAsync();

            // 5. Auditar
            await _audit.LogAsync(
                action: AuditAction.PermissionAssignedToRole,
                entity: "Role",
                userId: null, // Es una acción sobre un Rol, no un Usuario específico
                ipAddress: "N/A",
                userAgent: "API",
                additionalData: $"Role: {dto.RoleName}, Permission: {dto.Permission}");

            return Result.Success();
        }
    }
}
