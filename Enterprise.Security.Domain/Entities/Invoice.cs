using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class Invoice : BaseEntity
    {
        // --- Datos del Cliente ---
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty; // Snapshot

        // --- Trazabilidad (Claves para la Rúbrica) ---
        // Si viene de un pedido (Automática), esto tendrá valor:
        public Guid? OrderId { get; set; }

        // Si es venta directa, guardamos qué vendedor la hizo:
        public Guid? SellerId { get; set; }
        public string? SellerName { get; set; } // Snapshot del vendedor

        // --- Datos Fiscales ---
        public string InvoiceNumber { get; set; } = string.Empty; // Ej: "INV-2026-0001"
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Relaciones
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}
