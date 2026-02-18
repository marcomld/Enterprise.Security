using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Application.Interfaces.Authentication;
using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Infrastructure.Authorization;
using Enterprise.Security.Infrastructure.Identity;
using Enterprise.Security.Infrastructure.Persistence.DbContext;
using Enterprise.Security.Infrastructure.Persistence.Seed;
using Enterprise.Security.Infrastructure.Repositories;
using Enterprise.Security.Infrastructure.Services;
using Enterprise.Security.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Database
            services.AddDbContext<SecurityDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b =>
                    {
                        // Mantenemos esto que ya tenías (es importante para que las migraciones funcionen)
                        b.MigrationsAssembly(typeof(SecurityDbContext).Assembly.FullName);

                        // AGREGAMOS ESTO: Lógica de reintento ante fallos de conexión temporal
                        b.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }
                ));

            // 2. Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;

                // HARDENING 3: Bloqueo por intentos fallidos
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Bloqueado 15 min
                options.Lockout.MaxFailedAccessAttempts = 5; // Al 5to intento incorrecto, chau.
            })
            .AddEntityFrameworkStores<SecurityDbContext>()
            .AddDefaultTokenProviders();

            // 3. JWT Configuration (Vinculamos la clase JwtSettings)
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // 4. Inyectar Servicios (Lifetime: Scoped es lo estándar para servicios con BD)
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuditService, AuditService>();
            //
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();

            // 5. Inyectar Repositorios
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();

            // Repositorios de Inventario
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

            // Servicios de Inventario
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();

            // Repositorios y Servicios de Pedidos
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            // ... otros servicios
            services.AddScoped<ICurrentUserService, CurrentUserService>(); // <-- AGREGAR ESTO

            // --- AUTORIZACIÓN AVANZADA ---
            // Reemplazamos el proveedor de políticas por defecto con el nuestro dinámico
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // Registramos el manejador que verifica los claims
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Dentro de AddInfrastructure
            services.AddTransient<DbInitializer>();

            return services;
        }
    }
}
