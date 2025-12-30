using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Authentication
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(
            Guid userId,
            string userName,
            IEnumerable<string> roles,
            IEnumerable<string> permissions);

        // --- NUEVOS MÉTODOS ---
        string GenerateRefreshToken();

        // Este método permite leer los datos del usuario aunque el token haya expirado
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        // Agregamos esto para obtener la fecha exacta calculada desde configuración
        DateTime GetAccessTokenExpiration();
    }
}
