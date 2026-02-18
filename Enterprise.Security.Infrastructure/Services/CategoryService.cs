using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Inventory.Categories;
using Enterprise.Security.Application.Interfaces.Auditing; // Audit
using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Domain.Enums; // Enum

namespace Enterprise.Security.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IAuditService _audit;            // Inyección Audit
    private readonly ICurrentUserService _currentUser; // Inyección Usuario Actual

    public CategoryService(
        ICategoryRepository repository,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CategoryDto>>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        var dtos = categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IsActive)).ToList();
        return Result<List<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) return Result<CategoryDto>.Failure("Categoría no encontrada.");

        return Result<CategoryDto>.Success(new CategoryDto(category.Id, category.Name, category.Description, category.IsActive));
    }

    public async Task<Result<Guid>> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true
        };

        await _repository.AddAsync(category);

        // CORRECCIÓN: Argumentos nombrados para evitar errores de posición
        await _audit.LogAsync(
            action: AuditAction.CreateCategory,
            entity: "Category",
            userId: _currentUser.UserId,
            ipAddress: _currentUser.IpAddress, // IP correcta
            additionalData: $"Creó categoría: {category.Name}" // Dato correcto
        );

        return Result<Guid>.Success(category.Id);
    }

    public async Task<Result<string>> UpdateAsync(UpdateCategoryDto dto)
    {
        var category = await _repository.GetByIdAsync(dto.Id);
        if (category == null) return Result<string>.Failure("Categoría no encontrada.");

        // Guardamos nombre anterior para el log
        var oldName = category.Name;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;

        await _repository.UpdateAsync(category);

        await _audit.LogAsync(
            action: AuditAction.UpdateCategory,
            entity: "Category",
            userId: _currentUser.UserId,
            ipAddress: _currentUser.IpAddress,
            additionalData: $"Actualizó categoría ID: {category.Id}. Nombre anterior: {oldName}"
        );

        return Result<string>.Success("Categoría actualizada.");
    }

    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) return Result<string>.Failure("Categoría no encontrada.");

        await _repository.DeleteAsync(category);

        await _audit.LogAsync(
            action: AuditAction.DeleteCategory,
            entity: "Category",
            userId: _currentUser.UserId,
            ipAddress: _currentUser.IpAddress,
            additionalData: $"Eliminó categoría ID: {id}, Nombre: {category.Name}"
        );

        return Result<string>.Success("Categoría eliminada (lógicamente).");
    }
}