using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Auth
{
    public record RegisterRequestDto(
        string UserName,
        string Email,
        string Password,
        string FirstName,
        string LastName);
}
