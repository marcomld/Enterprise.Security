using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Inventory.Products
{
    public record ProductDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName, // Flattening: muy útil para grids en Blazor
    string SKU,
    string Name,
    string Description,
    decimal UnitPrice,
    decimal TaxRate,
    int StockQuantity,
    int MinStockLevel,
    string? ImageUrl,
    bool IsActive
);

    public record CreateProductDto(
        Guid CategoryId,
        string SKU,
        string Name,
        string Description,
        decimal UnitPrice,
        decimal TaxRate,
        int StockQuantity,
        int MinStockLevel,
        string? ImageUrl
    );

    public record UpdateProductDto(
        Guid Id,
        Guid CategoryId,
        string Name,
        string Description,
        decimal UnitPrice,
        decimal TaxRate,
        int MinStockLevel,
        string? ImageUrl,
        bool IsActive
    );

    // DTO especial para actualizar stock (usado por Ventas/Pedidos)
    public record AdjustStockDto(
        Guid ProductId,
        int QuantityAdjustment // Puede ser positivo (entrada) o negativo (salida)
    );
}
