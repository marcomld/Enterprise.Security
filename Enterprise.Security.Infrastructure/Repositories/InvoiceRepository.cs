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
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly SecurityDbContext _context;

        public InvoiceRepository(SecurityDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetByIdAsync(Guid id)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();
        }

        public async Task<int> GetNextInvoiceNumberAsync()
        {
            // Lógica simple: Contar cuántas facturas hay y sumar 1.
            // En un sistema real de alto tráfico, se usaría una secuencia de base de datos.
            return await _context.Invoices.CountAsync() + 1;
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }
    }
}
