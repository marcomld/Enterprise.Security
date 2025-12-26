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
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result<LoginResponseDto>> LoginAsync(RegisterRequestDto request);
        Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<Result> LogoutAsync(Guid userId, string refreshToken);
    }
}
