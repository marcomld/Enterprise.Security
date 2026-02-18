using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Orders
{
    public record CreateOrderDto(
    List<CreateOrderItemDto> Items,
    string? Notes
    );
}
