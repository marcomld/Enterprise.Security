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
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.ToTable("InvoiceItems");

            // Decimales
            builder.Property(ii => ii.UnitPrice).HasPrecision(18, 2);
            builder.Property(ii => ii.TaxRate).HasPrecision(18, 2);
            builder.Property(ii => ii.SubTotal).HasPrecision(18, 2);
            builder.Property(ii => ii.TaxAmount).HasPrecision(18, 2);
            builder.Property(ii => ii.Total).HasPrecision(18, 2);

            builder.Property(ii => ii.ProductSku).HasMaxLength(50);
            builder.Property(ii => ii.ProductName).HasMaxLength(200);
        }
    }
}
