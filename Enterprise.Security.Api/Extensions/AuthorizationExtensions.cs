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
    }
}
