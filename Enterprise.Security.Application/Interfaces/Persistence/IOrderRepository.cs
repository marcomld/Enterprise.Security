using Enterprise.Security.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Persistence
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task<List<Order>> GetAllAsync(); // Para Supervisor
        Task<List<Order>> GetByClientIdAsync(Guid clientId); // Para Clientes
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
