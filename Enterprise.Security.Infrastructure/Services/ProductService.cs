using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Inventory.Products;
using Enterprise.Security.Application.Interfaces.Auditing; // Audit
using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Domain.Enums; // Enum

namespace Enterprise.Security.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditService _audit;             // Inyección
    private readonly ICurrentUserService _currentUser; // Inyección

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var dtos = products.Select(MapToDto).ToList();
        return Result<List<ProductDto>>.Success(dtos);
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null
            ? Result<ProductDto>.Failure("Producto no encontrado.")
            : Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<ProductDto>> GetBySkuAsync(string sku)
    {
        var product = await _productRepository.GetBySkuAsync(sku);
        return product == null
            ? Result<ProductDto>.Failure("Producto no encontrado.")
            : Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<Guid>> CreateAsync(CreateProductDto dto)
    {
        if (!await _productRepository.IsSkuUniqueAsync(dto.SKU))
            return Result<Guid>.Failure($"El SKU '{dto.SKU}' ya existe.");

        if (!await _categoryRepository.ExistsAsync(dto.CategoryId))
            return Result<Guid>.Failure("La categoría especificada no existe.");

        var product = new Product
        {
            CategoryId = dto.CategoryId,
            SKU = dto.SKU,
            Name = dto.Name,
            Description = dto.Description,
            UnitPrice = dto.UnitPrice,
            TaxRate = dto.TaxRate,
            StockQuantity = dto.StockQuantity,
            MinStockLevel = dto.MinStockLevel,
            ImageUrl = dto.ImageUrl,
            IsActive = true
        };

        await _productRepository.AddAsync(product);

        // CORRECCIÓN: Argumentos nombrados
        await _audit.LogAsync(
            action: AuditAction.CreateProduct,
            entity: "Product",
            userId: _currentUser.UserId,
            ipAddress: _currentUser.IpAddress,
            additionalData: $"Creó producto: {product.Name}, SKU: {product.SKU}, Stock: {product.StockQuantity}"
        );

        return Result<Guid>.Success(product.Id);
    }

    public async Task<Result<string>> UpdateAsync(UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.Id);
        if (product == null) return Result<string>.Failure("Producto no encontrado.");

        if (!await _categoryRepository.ExistsAsync(dto.CategoryId))
            return Result<string>.Failure("La categoría especificada no existe.");

        // Log datos previos interesantes
        var logData = $"Cambios en producto {product.SKU}. Precio anterior: {product.UnitPrice}, Stock: {product.StockQuantity}";

        product.CategoryId = dto.CategoryId;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.UnitPrice = dto.UnitPrice;
        product.TaxRate = dto.TaxRate;
        product.MinStockLevel = dto.MinStockLevel;
        product.ImageUrl = dto.ImageUrl;
        product.IsActive = dto.IsActive;

        await _productRepository.UpdateAsync(product);

        await _audit.LogAsync(
            action: AuditAction.UpdateProduct,
            entity: "Product",
            userId: _currentUser.UserId,
            ipAddress: _currentUser.IpAddress,
            additionalData: $"Actualizó producto {product.SKU}. Precio: {product.UnitPrice}"
        );

        return Result<string>.Success("Producto actualizado exitosamente.");
    }

    public async Task<Result<string>> UpdateStockAsync(AdjustStockDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null) return Result<string>.Failure("Producto no encontrado.");

        int oldStock = product.StockQuantity;
        int newStock = product.StockQuantity + dto.QuantityAdjustment;

        if (newStock < 0) return Result<string>.Failure("Stock insuficiente para realizar esta operación.");

        product.StockQuantity = newStock;
        await _productRepository.UpdateAsync(product);

        await _productRepository.UpdateAsync(product);

        await _audit.LogAsync(
            action: AuditAction.AdjustStock,
            entity: "Product",
            userId: _currentUser.UserId,
            ipAddress: _currentUser.IpAddress,
            additionalData: $"Ajuste Stock: {dto.QuantityAdjustment}. Nuevo Total: {newStock}"
        );

        return Result<string>.Success($"Stock actualizado. Nuevo stock: {newStock}");
    }

    // Helper para mapeo manual
    private static ProductDto MapToDto(Product p) => new(
        p.Id,
        p.CategoryId,
        p.Category?.Name ?? "N/A",
        p.SKU,
        p.Name,
        p.Description,
        p.UnitPrice,
        p.TaxRate,
        p.StockQuantity,
        p.MinStockLevel,
        p.ImageUrl,
        p.IsActive
    );
}