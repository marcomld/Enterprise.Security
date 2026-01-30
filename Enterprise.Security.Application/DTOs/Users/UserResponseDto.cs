using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Users
{
    // Agregamos UserName, FirstName y LastName
    public record UserResponseDto(
        Guid Id,
        string UserName,
        string Email,
        string FirstName,
        string LastName,
        bool IsActive,
        List<string> Roles);
}
