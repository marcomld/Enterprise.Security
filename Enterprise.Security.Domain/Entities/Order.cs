using Enterprise.Security.Domain.Common;
using Enterprise.Security.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid ClientId { get; set; } // FK lógica
        public string ClientName { get; set; } = string.Empty; // Snapshot: Guardamos el nombre aquí para mostrarlo fácil

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Totales
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        // Relaciones

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
