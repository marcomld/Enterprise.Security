using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Roles;
using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Domain.Enums;
using Enterprise.Security.Infrastructure.Identity;
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
        private readonly IAuditService _audit;


        public RoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, // <--- Usa tu clase personalizada
            IAuditService audit)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _audit = audit;
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        }

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
    }
}
