using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<Result> AssignRoleAsync(AssignRoleDto dto); // Usando DTO mejor

        // Agregamos Listar Roles para que el Admin sepa qué asignar
        Task<List<string>> GetAllRolesAsync();
    }
}
