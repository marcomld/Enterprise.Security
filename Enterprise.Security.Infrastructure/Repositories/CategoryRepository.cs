using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly SecurityDbContext _context;

        public CategoryRepository(SecurityDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive) // Solo activas por defecto
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            // Implementamos borrado lógico para integridad referencial
            category.IsActive = false;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
