using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Inventory.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<Result<List<ProductDto>>> GetAllAsync();
        Task<Result<ProductDto>> GetByIdAsync(Guid id);
        Task<Result<ProductDto>> GetBySkuAsync(string sku);
        Task<Result<Guid>> CreateAsync(CreateProductDto dto);
        Task<Result<string>> UpdateAsync(UpdateProductDto dto);
        Task<Result<string>> UpdateStockAsync(AdjustStockDto dto); // Método clave para el futuro
        Task<Result<string>> DeleteAsync(Guid id); // <--- AGREGAR
        Task<Result<string>> ToggleStatusAsync(Guid id);
        Task<Result<string>> AdjustStockAsync(AdjustStockDto dto);
    }
}
