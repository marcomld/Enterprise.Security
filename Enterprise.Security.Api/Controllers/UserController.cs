using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [Authorize(Policy = Permissions.Users.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [Authorize(Policy = Permissions.Users.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _service.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [Authorize(Policy = Permissions.Users.Edit)]
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
            => Ok(await _service.ActivateAsync(id));

        [Authorize(Policy = Permissions.Users.Edit)]
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
            => Ok(await _service.DeactivateAsync(id));
    }

}
