using Enterprise.Security.Application.Common;
using Enterprise.Security.Infrastructure.Authorization;

namespace Enterprise.Security.Api.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
        {
            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // MAGIA: Obtenemos todos los permisos definidos en la clase estática
                var allPermissions = Permissions.GetAll();

                foreach (var permission in allPermissions)
                {
                    // Registramos una Policy con el mismo nombre que el permiso
                    options.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            return services;
        }
    }
}
