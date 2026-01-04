using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface IPermissionService
    {
        Task<Result> AssignPermissionToRoleAsync(AssignPermissionDto dto);
        Task<List<string>> GetAllPermissionsAsync(); //Para listar en el UI
    }
}
