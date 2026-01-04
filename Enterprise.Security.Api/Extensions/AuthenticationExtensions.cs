using Enterprise.Security.Infrastructure.Identity;
using Enterprise.Security.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Enterprise.Security.Api.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Leemos la configuración tipada para no cometer errores de string
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            if (jwtSettings == null) throw new ArgumentNullException(nameof(jwtSettings));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Dev only
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                    // HARDENING 1: ClockSkew Zero
                    // Por defecto Microsoft da 5 min de tolerancia. Lo quitamos para ser estrictos.
                    ClockSkew = TimeSpan.Zero
                };


                // HARDENING 2: Invalidación por SecurityStamp
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices
                            .GetRequiredService<UserManager<ApplicationUser>>();

                        var userId = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
                        var tokenStamp = context.Principal!.FindFirstValue("security_stamp");

                        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenStamp))
                        {
                            context.Fail("Token inválido: Faltan claims críticos");
                            return;
                        }

                        var user = await userManager.FindByIdAsync(userId);

                        // Si el usuario no existe O el sello cambió (ej. cambió password), rechazamos
                        if (user == null || user.SecurityStamp != tokenStamp)
                        {
                            context.Fail("Token expirado por seguridad (Sello inválido)");
                        }
                    }
                };
            });

            return services;
        }
    }
}
