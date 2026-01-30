using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Roles;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _service;

        public RolesController(IRoleService service)
        {
            _service = service;
        }

        [Authorize(Policy = Permissions.Roles.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _service.GetAllRolesAsync();
            return Ok(ApiResponse<List<RoleResponseDto>>.Ok(roles)); // Envuelve en ApiResponse
        }

        [Authorize(Policy = Permissions.Roles.Manage)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            var result = await _service.CreateRoleAsync(dto);
            return result.IsSuccess
                ? Ok(ApiResponse<string>.Ok("Rol creado"))
                : BadRequest(ApiResponse<string>.Fail(result.Error!));
        }

        [Authorize(Policy = Permissions.Roles.Manage)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeleteRoleAsync(id);
            return result.IsSuccess
                ? Ok(ApiResponse<string>.Ok("Rol eliminado"))
                : BadRequest(ApiResponse<string>.Fail(result.Error!));
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

        [Authorize(Policy = Permissions.Roles.Assign)]
        [HttpPost("unassign")]
        public async Task<IActionResult> Unassign([FromBody] AssignRoleDto dto)
        {
            var result = await _service.UnassignRoleAsync(dto);
            return result.IsSuccess
                ? Ok(ApiResponse<string>.Ok("Rol removido correctamente"))
                : BadRequest(ApiResponse<string>.Fail(result.Error!));
        }

        [Authorize(Policy = Permissions.Roles.View)]
        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetPermissions(string roleId)
        {
            var result = await _service.GetPermissionsByRoleAsync(roleId);
            return result.IsSuccess
                ? Ok(ApiResponse<List<PermissionDto>>.Ok(result.Value!))
                : BadRequest(ApiResponse<string>.Fail(result.Error!));
        }

        [Authorize(Policy = Permissions.Roles.Manage)]
        [HttpPut("permissions")]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdateRolePermissionsDto dto)
        {
            var result = await _service.UpdatePermissionsAsync(dto);
            return result.IsSuccess
                ? Ok(ApiResponse<string>.Ok("Permisos actualizados correctamente"))
                : BadRequest(ApiResponse<string>.Fail(result.Error!));
        }
    }
}
