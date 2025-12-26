using Enterprise.Security.Domain.Common;
using Enterprise.Security.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid? UserId { get; private set; }
        public AuditAction Action { get; private set; }
        public string Entity { get; private set; } = default!;
        public string? EntityId { get; private set; }
        public string IpAddress { get; private set; } = default!;
        public string UserAgent { get; private set; } = default!;
        public string? AdditionalData { get; private set; }

        private AuditLog() { }

        public AuditLog(Guid? userId, AuditAction action, string entity, string? entityId, string ipAddress, string userAgent, string? additionalData = null)
        {
            UserId = userId;
            Action = action;
            Entity = entity;
            EntityId = entityId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            AdditionalData = additionalData;
        }
    }
}
