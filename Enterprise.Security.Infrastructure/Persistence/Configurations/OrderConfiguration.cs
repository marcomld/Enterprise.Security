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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            // Configuraciones de decimales
            builder.Property(o => o.SubTotal).HasPrecision(18, 2);
            builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

            builder.Property(o => o.Notes).HasMaxLength(500);
            builder.Property(o => o.ClientName).HasMaxLength(200);
        }
    }
}
