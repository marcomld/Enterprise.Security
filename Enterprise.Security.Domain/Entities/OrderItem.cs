using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }

        // --- SNAPSHOT DATA (Datos congelados al momento de la compra) ---
        public string ProductSku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } // El % de impuesto que tenía ese día
                                             // ---------------------------------------------------------------

        public int Quantity { get; set; }

        // UnitPrice * Quantity
        public decimal SubTotal { get; set; }

        // (SubTotal * TaxRate)
        public decimal TaxAmount { get; set; }

        // SubTotal + TaxAmount
        public decimal Total { get; set; }

        // Relaciones
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
