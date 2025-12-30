using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Domain.Enums;
using Enterprise.Security.Infrastructure.Persistence.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        // OJO: Aquí inyectamos el DbContext directamente porque Audit es parte de la infraestructura
        private readonly SecurityDbContext _dbContext;

        public AuditService(SecurityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogAsync(
            AuditAction action,
            string entity,
            Guid? userId,
            string ipAddress,
            string userAgent,
            string? entityId = null,
            string? additionalData = null)
        {
            // ✅ USANDO ARGUMENTOS CON NOMBRE (Named Arguments)
            // Esto evita errores si cambias el orden en el futuro.
            // Sintaxis: parametro_del_constructor: variable_local

            var log = new AuditLog(
                userId: userId,
                action: action,
                entity: entity,
                entityId: entityId,
                ipAddress: ipAddress,
                userAgent: userAgent,
                additionalData: additionalData
            );

            _dbContext.AuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
