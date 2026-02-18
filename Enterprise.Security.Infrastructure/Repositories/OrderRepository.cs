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
    public class OrderRepository : IOrderRepository
    {
        private readonly SecurityDbContext _context;

        public OrderRepository(SecurityDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Items) // Eager Loading: Traer detalles
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            // Al añadir la cabecera, EF Core rastrea automáticamente los Items
            // y los insertará en la misma transacción implícita.
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
