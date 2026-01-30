using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Audit;
using Enterprise.Security.Application.Interfaces.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers
{
    [Route("api/audit")]
    [ApiController]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Audits.View)]
        public async Task<IActionResult> GetAll([FromQuery] string? search) // <--- Recibe parámetro por URL
        {
            var logs = await _auditService.GetAllLogsAsync(search);
            return Ok(ApiResponse<List<AuditLogResponseDto>>.Ok(logs));
        }
    }
}
