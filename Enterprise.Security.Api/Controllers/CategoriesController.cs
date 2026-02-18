using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Inventory.Categories;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Permissions.Categories.View)]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        // Usamos .Value en lugar de .Data
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Categories.View)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        // Usamos .IsSuccess y .Error
        if (!result.IsSuccess)
            return NotFound(ApiResponse<CategoryDto>.Fail(result.Error!));

        return Ok(ApiResponse<CategoryDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Permissions.Categories.Create)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(CreateCategoryDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Ok(ApiResponse<Guid>.Ok(result.Value));
    }

    [HttpPut("{id}")]
    [Authorize(Permissions.Categories.Edit)]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, UpdateCategoryDto dto)
    {
        if (id != dto.Id)
            return BadRequest(ApiResponse<string>.Fail("El ID de la URL no coincide con el cuerpo de la petición."));

        var result = await _service.UpdateAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok(result.Value!));
    }

    [HttpDelete("{id}")]
    [Authorize(Permissions.Categories.Delete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok(result.Value!));
    }
}