using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Auth
{
    // Agregamos AccessToken. El cliente debe enviar ambos.
    public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);
}
