using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public string SKU { get; set; } = string.Empty; // Código único
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } // Ej: 0.15 para 15%
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; } // Alerta de stock bajo
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Propiedad de Navegación
        public Category Category { get; set; } = null!;
    }
}
