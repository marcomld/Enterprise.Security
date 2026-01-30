using Enterprise.Security.Application.DTOs.Audit;
using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Domain.Enums;
using Enterprise.Security.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<AuditLogResponseDto>> GetAllLogsAsync(string? search = null)
        {
            var query = _dbContext.AuditLogs.AsQueryable();

            // Si hay texto de búsqueda, filtramos
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower(); // Normalizamos a minúsculas
                query = query.Where(x =>
                    x.Action.ToString().ToLower().Contains(search) ||
                    x.Entity.ToLower().Contains(search) ||
                    x.AdditionalData.ToLower().Contains(search) ||
                    x.UserId.ToString().Contains(search) // Búsqueda por ID de usuario
                                                         // Nota: Si UserId fuera Join con tabla Users, podrías buscar por Email, 
                                                         // pero por rendimiento ahora buscamos en lo que hay en la tabla Logs.
                );
            }

            // Traemos los últimos 200 logs ordenados por fecha descendente
            return await _dbContext.AuditLogs
                .OrderByDescending(x => x.CreatedAt)
                .Take(200)
                .Select(x => new AuditLogResponseDto(
                    x.Id,
                    x.UserId,
                    x.Action.ToString(),
                    x.Entity,
                    x.EntityId,
                    x.IpAddress,
                    x.AdditionalData,
                    x.CreatedAt
                ))
                .ToListAsync();
        }

        public Task<List<AuditLogResponseDto>> GetMyLogsAsync(string userId)
        {
            throw new NotImplementedException(); // Lo dejamos pendiente si no lo usas aún
        }
    }
}
