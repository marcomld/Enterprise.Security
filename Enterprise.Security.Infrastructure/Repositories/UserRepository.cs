using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var infraUser = await _userManager.FindByEmailAsync(email);
            return infraUser == null ? null : MapToDomain(infraUser);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            // Identity siempre maneja el ID como string en sus métodos de búsqueda
            var infraUser = await _userManager.FindByIdAsync(id.ToString());
            return infraUser == null ? null : MapToDomain(infraUser);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email);
        }

        public async Task UpdateAsync(User user)
        {
            var infraUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (infraUser == null) return;

            // --- MAPEO DE ACTUALIZACIÓN (Domain -> Infra) ---
            infraUser.FirstName = user.FirstName; // Nuevo
            infraUser.LastName = user.LastName;   // Nuevo
            infraUser.IsActive = user.IsActive;
            infraUser.LastLoginAt = user.LastLoginAt;

            await _userManager.UpdateAsync(infraUser);
        }

        // --- HELPER DE MAPEO (Infra -> Domain) ---
        private static User MapToDomain(ApplicationUser infraUser)
        {
            return User.LoadExisting(
                infraUser.Id,
                infraUser.FirstName,    // Nuevo
                infraUser.LastName,     // Nuevo
                infraUser.Email!,
                infraUser.UserName!,
                infraUser.IsActive,
                infraUser.LastLoginAt
            );
        }
    }
}
