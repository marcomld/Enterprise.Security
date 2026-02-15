using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Auth;
using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Application.Interfaces.Authentication;
using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Domain.Enums;
using Enterprise.Security.Infrastructure.Identity;
using Enterprise.Security.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IRoleRepository _roleRepo;
        private readonly IPermissionRepository _permRepo;
        private readonly IAuditService _audit;
        private readonly JwtSettings _jwtSettings; // Configuracion inyectada

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IRoleRepository roleRepo,
            IPermissionRepository permRepo,
            IAuditService audit,
            IOptions<JwtSettings> jwtOptions)  //Inyeccion de opciones
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _roleRepo = roleRepo;
            _permRepo = permRepo;
            _audit = audit;
            _jwtSettings = jwtOptions.Value;
        }

        // ---------------------------------------------------------
        // LOGIN
        // ---------------------------------------------------------
        public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            // 1. Buscar usuario por email
            var appUser = await _userManager.FindByEmailAsync(request.Email);

            if (appUser is null)
            {
                // Dummy delay para mitigar "Timing Attacks" (evita que adivinen emails válidos por el tiempo de respuesta)
                await Task.Delay(new Random().Next(100, 200));
                return Result<LoginResponseDto>.Failure("Credenciales inválidas");
            }

            // 2. Validar si está activo (Lógica de Negocio)
            if (!appUser.IsActive)
                return Result<LoginResponseDto>.Failure("El usuario está inactivo. Contacte al administrador.");

            // 3. Validar Password + Protección de Bloqueo (Lockout)
            // lockoutOnFailure: true -> Incrementa el contador de fallos. Si llega al límite (default 5), bloquea al usuario.
            var result = await _signInManager.CheckPasswordSignInAsync(appUser, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                string errorMessage = "Credenciales inválidas";
                string auditData = "Contraseña incorrecta";

                if (result.IsLockedOut)
                {
                    auditData = "Cuenta bloqueada";

                    // 1. Obtenemos la fecha exacta de fin de bloqueo desde Identity
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(appUser);

                    if (lockoutEnd.HasValue)
                    {
                        // 2. Calculamos el tiempo restante
                        var remaining = lockoutEnd.Value - DateTimeOffset.UtcNow;

                        // 3. Formateamos el mensaje amigable
                        if (remaining.TotalMinutes >= 1)
                        {
                            errorMessage = $"Cuenta bloqueada. Inténtalo de nuevo en {Math.Ceiling(remaining.TotalMinutes)} minutos.";
                        }
                        else
                        {
                            errorMessage = $"Cuenta bloqueada. Inténtalo de nuevo en {Math.Ceiling(remaining.TotalSeconds)} segundos.";
                        }
                    }
                    else
                    {
                        errorMessage = "Cuenta bloqueada temporalmente.";
                    }
                }

                await _audit.LogAsync(
                    action: AuditAction.FailedLogin,
                    entity: "User",
                    userId: appUser.Id,
                    //ipAddress: "N/A",
                    //userAgent: "API",
                    additionalData: auditData);

                return Result<LoginResponseDto>.Failure(errorMessage);
            }

            // 4. Éxito: Generar Tokens y Respuesta
            // CAMBIO: Pasamos explícitamente que esto es un LOGIN
            return await GenerateAuthResponseAsync(appUser, AuditAction.Login);
        }


        // ---------------------------------------------------------
        // REGISTRO
        // ---------------------------------------------------------
        public async Task<Result<LoginResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            // 1. Validaciones previas
            var existingUserEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserEmail != null)
                return Result<LoginResponseDto>.Failure($"El email {request.Email} ya está registrado.");

            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null)
                return Result<LoginResponseDto>.Failure($"El usuario {request.UserName} ya existe.");

            // 2. Crear Entidad de Infraestructura
            var newUser = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString() // Inicializamos el sello
            };

            // 3. Guardar en BD (Identity hashea el password automáticamente)
            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<LoginResponseDto>.Failure($"Error al registrar: {errors}");
            }

            // 4. Asignar Rol por defecto ("User")
            // Asegúrate de que este rol exista en tu tabla AspNetRoles, si no, fallará.
            await _userManager.AddToRoleAsync(newUser, "User");

            // 5. Auditoría
            await _audit.LogAsync(
            action: AuditAction.UserCreated,
            entity: "User",
            userId: newUser.Id,
            //ipAddress: "N/A",
            //userAgent: "API",
            additionalData: "Nuevo usuario registrado");

            // 6. Auto-Login (Devolver tokens inmediatamente)
            return await GenerateAuthResponseAsync(newUser, AuditAction.UserCreated);
        }


        // ---------------------------------------------------------
        // REFRESH TOKEN
        // ---------------------------------------------------------
        public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // 1. Obtener el "Principal" (Usuario) del AccessToken expirado
            // Esto nos dice quién está intentando refrescar el token
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            if (principal == null)
                return Result<LoginResponseDto>.Failure("Token de acceso inválido.");

            // El ID del usuario suele estar en el claim "NameIdentifier" o "Sub"
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? principal.FindFirstValue(ClaimTypes.Name) // Fallback
                               ?? principal.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userIdString))
                return Result<LoginResponseDto>.Failure("Token inválido: No se encontró identificación.");

            var appUser = await _userManager.FindByIdAsync(userIdString);

            // 2. Validaciones de Seguridad del Refresh Token
            if (appUser == null ||
                appUser.RefreshToken != request.RefreshToken ||
                appUser.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result<LoginResponseDto>.Failure("Refresh Token inválido o expirado. Inicie sesión nuevamente.");
            }

            // 3. Rotación de Tokens (Seguridad: Usar y Desechar)
            // Generamos nuevos tokens y reemplazamos el anterior en la BD
            return await GenerateAuthResponseAsync(appUser, AuditAction.TokenRefreshed);
        }


        // ---------------------------------------------------------
        // LOGOUT
        // ---------------------------------------------------------
        public async Task<Result> LogoutAsync(Guid userId, string refreshToken)
        {
            // Buscamos al usuario para revocar su Refresh Token
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user != null)
            {
                // Limpiamos el token en BD para que no pueda volver a usarse
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            // CORRECCIÓN: Aquí tenías un bug. "Cierre de sesión" se iba al campo userAgent.
            // Ahora usamos argumentos con nombre para asegurarnos de que va a additionalData.
            await _audit.LogAsync(
                action: AuditAction.Logout,
                entity: "User",
                userId: userId,
                additionalData: "Cierre de sesión");

            return Result.Success();
        }


        // ---------------------------------------------------------
        // HELPER: GENERACIÓN DE RESPUESTA
        // ---------------------------------------------------------
        // --- MÉTODO HELPER PRIVADO (DRY) ---
        // Centraliza la lógica de generación de tokens y respuesta
        private async Task<Result<LoginResponseDto>> GenerateAuthResponseAsync(ApplicationUser user, AuditAction auditAction)
        {
            // A. Obtener Roles y Permisos actualizados
            var roles = await _roleRepo.GetRolesByUserIdAsync(user.Id);
            var permissions = await _permRepo.GetPermissionsByUserIdAsync(user.Id);

            // HARDENING: Obtenemos el Sello de Seguridad actual
            var securityStamp = await _userManager.GetSecurityStampAsync(user);

            // B. Generar Tokens JWT
            var accessToken = await _tokenService.GenerateAccessTokenAsync(
                user.Id,
                user.UserName!,
                roles,
                permissions,
                securityStamp ?? "default_stamp");

            var refreshToken = _tokenService.GenerateRefreshToken();

            // Obtener expiración dinámica desde configuración (AJUSTE 2)
            var accessTokenExpiration = _tokenService.GetAccessTokenExpiration();

            // C. Guardar Refresh Token en BD
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Duración del Refresh Token
            user.LastLoginAt = DateTime.UtcNow; // Actualizamos última conexión

            await _userManager.UpdateAsync(user);

            // Auditoría dinámica (AJUSTE 1)
            // Ya no dice siempre "Login", ahora dirá "Create" o "TokenRefreshed" según el caso
            // CORRECCIÓN: Quitamos parámetros manuales
            await _audit.LogAsync(
                action: auditAction,
                entity: "User",
                userId: user.Id,
                additionalData: $"Acción exitosa: {auditAction}");

            return Result<LoginResponseDto>.Success(
                new LoginResponseDto(
                    user.Id,
                    user.UserName!,
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    roles.ToList(),
                    accessToken,
                    refreshToken,
                    accessTokenExpiration // Usamos la fecha dinámica
                ));
        }
    }
}
