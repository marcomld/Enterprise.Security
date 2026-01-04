using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Roles;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers
{
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _service;

        public RolesController(IRoleService service)
        {
            _service = service;
        }

        [Authorize(Policy = Permissions.Roles.Assign)]
        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignRoleDto dto)
        {
            var result = await _service.AssignRoleAsync(dto);
            return result.IsSuccess
                ? Ok(ApiResponse<string>.Ok("Rol asignado correctamente"))
                : BadRequest(ApiResponse<string>.Fail(result.Error!));
        }
    }
}
