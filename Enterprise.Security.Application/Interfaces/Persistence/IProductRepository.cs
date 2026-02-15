using Enterprise.Security.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Persistence
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product?> GetBySkuAsync(string sku); // Vital para buscar rápido
        Task<List<Product>> GetAllAsync();
        Task<bool> IsSkuUniqueAsync(string sku);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        // No ponemos Delete, preferimos desactivar productos con historial
    }
}
