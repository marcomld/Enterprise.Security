using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Users;
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
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _audit;

        public UserService(UserManager<ApplicationUser> userManager, IAuditService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            return await _userManager.Users
                .Select(u => new UserResponseDto(
                    u.Id,
                    u.Email!,
                    u.IsActive
                )).ToListAsync();
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            return new UserResponseDto(
                user.Id,
                user.Email!,
                user.IsActive
            );
        }

        public async Task<Result> ActivateAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return Result.Failure("Usuario no encontrado");

            user.IsActive = true; // Setter directo
            await _userManager.UpdateAsync(user);

            await _audit.LogAsync(
                action: AuditAction.UserActivated,
                entity: "User",
                userId: id,
                ipAddress: "N/A",
                userAgent: "API");

            return Result.Success();
        }

        public async Task<Result> DeactivateAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return Result.Failure("Usuario no encontrado");

            // Evitar auto-desactivación (seguridad básica)
            // if (es_el_mismo_usuario_logueado) return Result.Failure("No puedes desactivarte a ti mismo");

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            await _audit.LogAsync(
                action: AuditAction.UserDeactivated,
                entity: "User",
                userId: id,
                ipAddress: "N/A",
                userAgent: "API");

            return Result.Success();
        }
    }
}
