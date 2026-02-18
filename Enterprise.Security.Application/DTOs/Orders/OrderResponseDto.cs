using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Orders
{
    public record OrderResponseDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    DateTime OrderDate,
    string Status, // Devolvemos el string del Enum (Pending, Approved...)
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string? Notes,
    List<OrderItemResponseDto> Items
    );
}
