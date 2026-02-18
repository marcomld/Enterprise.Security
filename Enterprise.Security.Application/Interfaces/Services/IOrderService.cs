using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Result<Guid>> CreateOrderAsync(CreateOrderDto dto);
        Task<Result<List<OrderResponseDto>>> GetMyOrdersAsync();
        Task<Result<List<OrderResponseDto>>> GetAllOrdersAsync(); // Supervisor
        Task<Result<OrderResponseDto>> GetOrderByIdAsync(Guid id);
        Task<Result<string>> ApproveOrderAsync(Guid orderId); // EL MÉTODO CRÍTICO
    }
}
