using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.DTOs.Inventory.Categories
{
    public record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive
    );

    public record CreateCategoryDto(
        string Name,
        string Description
    );

    public record UpdateCategoryDto(
        Guid Id,
        string Name,
        string Description,
        bool IsActive
    );
}
