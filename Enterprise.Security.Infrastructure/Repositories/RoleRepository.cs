using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<string>> GetRolesByUserIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) {
                return Enumerable.Empty<string>();
            }

            return await _userManager.GetRolesAsync(user);
        }
    }
}
