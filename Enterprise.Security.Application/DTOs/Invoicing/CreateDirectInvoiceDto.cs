using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Invoicing
{
    public record CreateDirectInvoiceItemDto(
    Guid ProductId,
    int Quantity
    );

    public record CreateDirectInvoiceDto(
        Guid ClientId, // El vendedor elige a qué cliente le vende
        List<CreateDirectInvoiceItemDto> Items
    );
}
