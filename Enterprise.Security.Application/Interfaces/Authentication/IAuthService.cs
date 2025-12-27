using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Authentication
{
    public interface IAuthService
    {
        // Login recibe LoginRequestDto
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);

        // Register recibe RegisterRequestDto
        Task<Result<LoginResponseDto>> RegisterAsync(RegisterRequestDto request);

        // Refresh recibe RefreshTokenRequestDto
        Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);

        Task<Result> LogoutAsync(Guid userId, string refreshToken);
    }
}
