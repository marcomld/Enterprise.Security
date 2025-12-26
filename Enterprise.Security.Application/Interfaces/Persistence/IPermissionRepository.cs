using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Persistence
{
    public interface IPermissionRepository
    {
      Task<IEnumerable<string>> GetPermissionsByRoleIdAsync(Guid roleId);
    }
}
