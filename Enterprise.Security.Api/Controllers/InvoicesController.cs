using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Invoicing;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _service;

        public InvoicesController(IInvoiceService service)
        {
            _service = service;
        }

        [HttpGet("my-invoices")]
        [Authorize(Permissions.Invoices.ViewMy)]
        public async Task<ActionResult<ApiResponse<List<InvoiceResponseDto>>>> GetMyInvoices()
        {
            var result = await _service.GetMyInvoicesAsync();
            return Ok(ApiResponse<List<InvoiceResponseDto>>.Ok(result.Value!));
        }

        [HttpGet]
        [Authorize(Permissions.Invoices.ViewAll)]
        public async Task<ActionResult<ApiResponse<List<InvoiceResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllInvoicesAsync();
            return Ok(ApiResponse<List<InvoiceResponseDto>>.Ok(result.Value!));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<InvoiceResponseDto>.Fail(result.Error!));
            return Ok(ApiResponse<InvoiceResponseDto>.Ok(result.Value!));
        }

        [HttpPost("direct")]
        [Authorize(Permissions.Invoices.CreateDirect)]
        public async Task<ActionResult<ApiResponse<Guid>>> CreateDirect(CreateDirectInvoiceDto dto)
        {
            var result = await _service.CreateDirectInvoiceAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<Guid>.Fail(result.Error!));
            return Ok(ApiResponse<Guid>.Ok(result.Value));
        }
    }
}
