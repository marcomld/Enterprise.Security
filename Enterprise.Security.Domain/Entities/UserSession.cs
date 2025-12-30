using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class UserSession : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid RefreshTokenId { get; private set; }
        public string IpAddress { get; private set; } = default!;
        public string UserAgent { get; private set; } = default!;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }

        private UserSession() { }

        public UserSession(Guid userId, Guid refreshTokenId, string ipAddress, string userAgent, DateTime expiresAt)
        {
            UserId = userId;
            RefreshTokenId = refreshTokenId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ExpiresAt = expiresAt;
        }

        public void Revoke()
        {
            IsRevoked = true;
        }
    }
}
