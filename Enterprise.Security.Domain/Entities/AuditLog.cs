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
        public Guid? UserId { get; set; }
        public AuditAction Action { get; set; }
        public string Entity { get; set; } = string.Empty;
        public string? EntityId { get; set; } // ID del registro afectado (PK)
        
        // CORRECCIÓN: Aseguramos que estas propiedades existan y se asignen
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }

        // Constructor vacío para EF Core
        protected AuditLog() { }

        // Constructor principal con NOMBRES claros
        public AuditLog(
            Guid? userId,
            AuditAction action,
            string entity,
            string? entityId,
            string ipAddress,
            string userAgent,
            string? additionalData)
        {
            UserId = userId;
            Action = action;
            Entity = entity;
            EntityId = entityId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            AdditionalData = additionalData;
            CreatedAt = DateTime.UtcNow; // Aseguramos fecha de creación
        }
    }
}
