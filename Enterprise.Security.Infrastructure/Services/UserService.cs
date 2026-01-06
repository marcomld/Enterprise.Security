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
                    u.UserName!,     // Nuevo
                    u.Email!,
                    u.FirstName,     // Nuevo
                    u.LastName,      // Nuevo
                    u.IsActive
                )).ToListAsync();
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            return new UserResponseDto(
                user.Id,
                user.UserName!,     // Nuevo
                user.Email!,
                user.FirstName,     // Nuevo
                user.LastName,      // Nuevo
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
                userAgent: "API",
                additionalData: "Usuario Activado");

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
                userAgent: "API",
                additionalData: "Usuario Desactivado");

            return Result.Success();
        }

        public async Task<Result<string>> CreateAsync(CreateUserDto request)
        {
            // 1. Validaciones de unicidad
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null) return Result<string>.Failure("El email ya está registrado.");

            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null) return Result<string>.Failure("El nombre de usuario ya existe.");

            // 2. Crear Entidad (Mapeo desde el record)
            var newUser = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // 3. Guardar en Identity
            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<string>.Failure(errors);
            }

            // 4. Asignar Rol por defecto ("User")
            await _userManager.AddToRoleAsync(newUser, "User");

            // 5. Auditoría
            await _audit.LogAsync(
                action: AuditAction.UserCreated,
                entity: "User",
                userId: newUser.Id,
                ipAddress: "N/A", // Podrías pasar esto desde el controller si quisieras ser estricto
                userAgent: "Admin Panel",
                additionalData: "Nuevo usuario creado"
            );

            return Result<string>.Success(newUser.Id.ToString());
        }
    }
}
