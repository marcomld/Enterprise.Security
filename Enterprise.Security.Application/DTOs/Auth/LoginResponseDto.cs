using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Auth
{
    public record LoginResponseDto(
         Guid Id,              // Útil para el frontend
         string UserName,      // Para mostrar "Hola, user"
         string Email,
         string FirstName,     // Para mostrar "Hola, Juan"
         string LastName,
         List<string> Roles,   // Muy útil para ocultar botones en el front según permisos
         string AccessToken,
         string RefreshToken,
         DateTime ExpiresAt);
}
