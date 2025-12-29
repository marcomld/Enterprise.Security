using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Authorization
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // 1. Revisa si ya existe una política con ese nombre (ej: "AdminOnly")
            var policy = await base.GetPolicyAsync(policyName);

            if (policy != null) return policy;

            // 2. Si no existe, asume que el 'policyName' es un código de permiso (ej: "users.create")
            // y crea una política dinámica para validarlo.
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}
