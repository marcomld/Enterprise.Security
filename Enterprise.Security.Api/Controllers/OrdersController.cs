using Enterprise.Security.Api.Models;
using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Orders;
using Enterprise.Security.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Security.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Permissions.Orders.Create)]
        public async Task<ActionResult<ApiResponse<Guid>>> Create(CreateOrderDto dto)
        {
            var result = await _service.CreateOrderAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse<Guid>.Fail(result.Error!));

            return Ok(ApiResponse<Guid>.Ok(result.Value));
        }

        [HttpGet("my-orders")]
        [Authorize(Permissions.Orders.ViewMy)]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetMyOrders()
        {
            var result = await _service.GetMyOrdersAsync();
            return Ok(ApiResponse<List<OrderResponseDto>>.Ok(result.Value!));
        }

        [HttpGet]
        [Authorize(Permissions.Orders.ViewAll)]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllOrdersAsync();
            return Ok(ApiResponse<List<OrderResponseDto>>.Ok(result.Value!));
        }

        [HttpGet("{id}")]
        [Authorize] // Permiso validado internamente o genérico
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetById(Guid id)
        {
            var result = await _service.GetOrderByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(ApiResponse<OrderResponseDto>.Fail(result.Error!));

            return Ok(ApiResponse<OrderResponseDto>.Ok(result.Value!));
        }

        [HttpPut("{id}/approve")]
        [Authorize(Permissions.Orders.Approve)]
        public async Task<ActionResult<ApiResponse<string>>> Approve(Guid id)
        {
            var result = await _service.ApproveOrderAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse<string>.Fail(result.Error!));

            return Ok(ApiResponse<string>.Ok(result.Value!));
        }
    }
}
