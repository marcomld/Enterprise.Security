using Enterprise.Security.Application.Interfaces.Authentication;
using Enterprise.Security.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<string> GenerateAccessTokenAsync(
            Guid userId,
            string userName,
            IEnumerable<string> roles,
            IEnumerable<string> permissions,
            string securityStamp)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()), // Estándar de .NET
            new(JwtRegisteredClaimNames.Sub, userId.ToString()), // Estándar JWT
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("security_stamp", securityStamp) // <--- CRÍTICO: Agregamos el sello
        };

            // Agregamos Claims estándar
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            // Agregamos Permisos como claims personalizados
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
                signingCredentials: creds);

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public string GenerateRefreshToken()
        {
            // Genera un token aleatorio seguro para refresh token
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // --- NUEVA IMPLEMENTACIÓN CRÍTICA ---
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true, // Debe coincidir con tu config
                ValidAudience = _settings.Audience,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),

                // ¡ESTO ES LO IMPORTANTE! Ignoramos que esté vencido para poder leerlo
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                // Validación extra de seguridad: asegurar que se usó HmacSha256
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch
            {
                // Si el token es basura o no se puede leer, retornamos null
                return null;
            }
        }

        // NUEVO MÉTODO IMPLEMENTADO
        public DateTime GetAccessTokenExpiration()
        {
            return DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);
        }
    }
}
