using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Inventory.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<Result<List<CategoryDto>>> GetAllAsync();
        Task<Result<CategoryDto>> GetByIdAsync(Guid id);
        Task<Result<Guid>> CreateAsync(CreateCategoryDto dto);
        Task<Result<string>> UpdateAsync(UpdateCategoryDto dto);
        Task<Result<string>> DeleteAsync(Guid id);
    }
}
