using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Invoicing
{
    public record InvoiceItemDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TaxRate,
    decimal SubTotal,
    decimal TaxAmount,
    decimal Total
    );

    public record InvoiceResponseDto(
        Guid Id,
        string InvoiceNumber,
        DateTime IssuedDate,
        string ClientName,
        string? SellerName, // Null si fue automática
        Guid? OrderId,      // Null si fue directa
        decimal SubTotal,
        decimal TaxAmount,
        decimal TotalAmount,
        List<InvoiceItemDto> Items
    );
}
