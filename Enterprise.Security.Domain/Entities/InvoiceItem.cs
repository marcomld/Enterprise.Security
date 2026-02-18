using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public Guid ProductId { get; set; }

        // Snapshot Data
        public string ProductSku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TaxRate { get; set; } // % de impuesto aplicado

        // Cálculos
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }

        // Relaciones
        public Invoice Invoice { get; set; } = null!;
    }
}
