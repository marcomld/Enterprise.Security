using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Inventory.Products;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Permissions.Products.View)]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Products.View)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<ProductDto>.Fail(result.Error!));

        return Ok(ApiResponse<ProductDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Permissions.Products.Create)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(CreateProductDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<Guid>.Fail(result.Error!));

        return Ok(ApiResponse<Guid>.Ok(result.Value));
    }

    [HttpPut("{id}")]
    [Authorize(Permissions.Products.Edit)]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, UpdateProductDto dto)
    {
        if (id != dto.Id)
            return BadRequest(ApiResponse<string>.Fail("ID mismatch"));

        var result = await _service.UpdateAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok(result.Value!));
    }

    [HttpDelete("{id}")]
    [Authorize(Permissions.Products.Delete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok(result.Value!));
    }

    [HttpPut("{id}/toggle-status")]
    [Authorize(Permissions.Products.Edit)]
    public async Task<ActionResult<ApiResponse<string>>> ToggleStatus(Guid id)
    {
        var result = await _service.ToggleStatusAsync(id);
        if (!result.IsSuccess) return BadRequest(ApiResponse<string>.Fail(result.Error!));
        return Ok(ApiResponse<string>.Ok(result.Value!));
    }

    [HttpPut("adjust-stock")]
    [Authorize(Permissions.Products.Edit)]
    public async Task<ActionResult<ApiResponse<string>>> AdjustStock(AdjustStockDto dto)
    {
        var result = await _service.AdjustStockAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok(result.Value!));
    }
}