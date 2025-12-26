using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Auth
{
    public record LoginResponseDto(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt);
}
