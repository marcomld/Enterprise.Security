using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly SecurityDbContext _dbContext;

        public PermissionRepository(SecurityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<string>> GetPermissionsByUserIdAsync(Guid userId)
        {
            // Usamos sintaxis de Query limpia
            var query = from ur in _dbContext.UserRoles
                        join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
                        join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                        where ur.UserId == userId
                        select p.Code;

            // .ToListAsync() devuelve List<string>, que es compatible con IEnumerable<string>
            var permissions = await query.Distinct().ToListAsync();

            return permissions;
        }
    }
}
