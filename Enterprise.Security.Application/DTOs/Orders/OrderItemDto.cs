using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Orders
{
    public record CreateOrderItemDto(
    Guid ProductId,
    int Quantity
    );

    public record OrderItemResponseDto(
        Guid ProductId,
        string ProductSku,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal SubTotal,
        decimal TaxAmount,
        decimal Total
    );
}
