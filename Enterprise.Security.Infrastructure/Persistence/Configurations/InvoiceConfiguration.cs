using Enterprise.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            // El número de factura debe ser único
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            builder.Property(i => i.InvoiceNumber).HasMaxLength(20).IsRequired();

            builder.Property(i => i.ClientName).HasMaxLength(200);
            builder.Property(i => i.SellerName).HasMaxLength(200);

            // Decimales (Moneda)
            builder.Property(i => i.SubTotal).HasPrecision(18, 2);
            builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
            builder.Property(i => i.TotalAmount).HasPrecision(18, 2);

            // Relación opcional con Order (Uno a Uno opcional)
            // No definimos la navegación en la entidad Order para evitar ciclos, 
            // pero aquí podemos dejar constancia de la FK si quisieras integridad referencial estricta.
            // Por ahora, lo manejamos como FK lógica para simplificar.
        }
    }
}
