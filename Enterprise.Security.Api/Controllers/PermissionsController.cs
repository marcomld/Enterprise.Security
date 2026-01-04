using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Permissions;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _service;

        public PermissionsController(IPermissionService service)
        {
            _service = service;
        }

        [Authorize(Policy = Permissions.SystemPermissions.Manage)]
        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignPermissionDto dto)
        {
            var result = await _service.AssignPermissionToRoleAsync(dto);
            return result.IsSuccess ? Ok(ApiResponse<string>.Ok("Permiso asignado")) : BadRequest(ApiResponse<string>.Fail(result.Error!));
        }
    }

}
