using Enterprise.Security.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Persistence
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(Guid id);
        Task<List<Invoice>> GetAllAsync(); // Supervisor
        Task<List<Invoice>> GetByClientIdAsync(Guid clientId); // Cliente

        // Método clave para generar el número secuencial (Ej: INV-0001, INV-0002)
        Task<int> GetNextInvoiceNumberAsync();

        Task AddAsync(Invoice invoice);
    }
}
