using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) // Standard JWT "sub"
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("uid"); // Fallback

                return idClaim != null ? Guid.Parse(idClaim) : Guid.Empty;
            }
        }

        public string IpAddress
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return "N/A";

                // Intentar obtener IP real si hay proxy (X-Forwarded-For)
                if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                    return context.Request.Headers["X-Forwarded-For"].ToString();

                return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
            }
        }
    }
}
