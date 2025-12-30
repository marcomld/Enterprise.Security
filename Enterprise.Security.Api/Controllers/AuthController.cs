using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.DTOs.Auth;
using Enterprise.Security.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Enterprise.Security.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            // Usamos el wrapper ApiResponse para estandarizar
            return result.IsSuccess
                ? Ok(ApiResponse<LoginResponseDto>.Ok(result.Value!))
                : Unauthorized(ApiResponse<LoginResponseDto>.Fail(result.Error!));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);

            return result.IsSuccess
                ? Ok(ApiResponse<LoginResponseDto>.Ok(result.Value!))
                : BadRequest(ApiResponse<LoginResponseDto>.Fail(result.Error!));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            return result.IsSuccess
                ? Ok(ApiResponse<LoginResponseDto>.Ok(result.Value!))
                : Unauthorized(ApiResponse<LoginResponseDto>.Fail(result.Error!));
        }

        [HttpPost("logout")]
        [Authorize] // Solo usuarios logueados pueden cerrar sesión
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            // Obtenemos el ID del usuario desde el Token (Claims)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(ApiResponse<string>.Fail("Token inválido"));

            var result = await _authService.LogoutAsync(userId, request.RefreshToken);

            return result.IsSuccess
                ? Ok(ApiResponse<string>.Ok("Sesión cerrada correctamente"))
                : BadRequest(ApiResponse<string>.Fail(result.Error!));
        }
    }
}
