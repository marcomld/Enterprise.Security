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
using Microsoft.AspNetCore.Http; // Para IHttpContextAccessor

namespace Enterprise.Security.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        // OJO: Aquí inyectamos el DbContext directamente porque Audit es parte de la infraestructura
        private readonly SecurityDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(SecurityDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        AuditAction action,
        string entity,
        Guid? userId,
        string ipAddress = "", // Valor por defecto vacío para que sea opcional
        string userAgent = "", // Valor por defecto vacío
        string? entityId = null,
        string? additionalData = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

            // 1. Detección Automática Robustecida
            if (string.IsNullOrEmpty(ipAddress) && httpContext != null)
            {
                // Intentamos obtener la IP real si hay proxy, sino la remota
                ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "::1";
            }

            // 2. Detección Automática de UserAgent si no viene informado
            if (string.IsNullOrEmpty(userAgent) && httpContext != null)
        {
            userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        }

        // 3. Fallback final por si todo falla
        if (string.IsNullOrEmpty(ipAddress)) ipAddress = "0.0.0.0";
        if (string.IsNullOrEmpty(userAgent)) userAgent = "Unknown";

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
            // Hacemos Join con la tabla de Usuarios para obtener el Email
            var query = from log in _dbContext.AuditLogs.AsNoTracking()
                        join user in _dbContext.Users.AsNoTracking()
                        on log.UserId equals user.Id into userJoin
                        from u in userJoin.DefaultIfEmpty() // Left Join (por si el usuario se borró)
                        select new
                        {
                            Log = log,
                            UserEmail = u != null ? u.Email : "Desconocido"
                        };

            // NOTA: Hemos quitado la lógica del "if (!string.IsNullOrWhiteSpace(search))" 
            // para limpiar el código ya que decidiste no usar la búsqueda por ahora.

            var result = await query
                .OrderByDescending(x => x.Log.CreatedAt)
                .Take(200)
                .Select(x => new AuditLogResponseDto(
                    x.Log.Id,
                    x.Log.UserId,
                    x.UserEmail, // <--- Aquí asignamos el email recuperado
                    x.Log.Action.ToString(),
                    x.Log.Entity,
                    x.Log.EntityId,
                    x.Log.IpAddress,
                    x.Log.UserAgent, // <--- Faltaba este parámetro
                    x.Log.AdditionalData, // <--- Faltaba este parámetro
                    x.Log.CreatedAt
                ))
                .ToListAsync();

            return result;
        }

        public Task<List<AuditLogResponseDto>> GetMyLogsAsync(string userId)
        {
            throw new NotImplementedException(); // Lo dejamos pendiente si no lo usas aún
        }
    }
}
