using Enterprise.Security.Application.DTOs.Audit;
using Enterprise.Security.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Auditing
{
    public interface IAuditService
    {
        Task LogAsync(
            AuditAction action,
            string entity,
            Guid? userId,
            string ipAddress,
            string userAgent,
            string? entityId = null,
            string? additionalData = null);

        // NUEVO: Método para leer logs (limitado a los últimos 100 por rendimiento inicial)
        Task<List<AuditLogResponseDto>> GetMyLogsAsync(string userId); // Opcional: ver mis propios logs
        //Task<List<AuditLogResponseDto>> GetAllLogsAsync(); // Para el Auditor
        Task<List<AuditLogResponseDto>> GetAllLogsAsync(string? search = null);
    }
}
