using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Users
{
    // Usamos record posicional. 
    // Nota: ASP.NET Core 8 bindea el body JSON al constructor automáticamente.
    public record CreateUserDto(
        [Required] string FirstName,
        [Required] string LastName,
        [Required] string UserName,
        [Required, EmailAddress] string Email,
        [Required] string Password
    );
}
