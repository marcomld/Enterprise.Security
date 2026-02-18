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
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            // Relación con Producto
            builder.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // No borrar producto si está en pedidos

            // Decimales
            builder.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            builder.Property(oi => oi.TaxRate).HasPrecision(18, 2);
            builder.Property(oi => oi.SubTotal).HasPrecision(18, 2);
            builder.Property(oi => oi.TaxAmount).HasPrecision(18, 2);
            builder.Property(oi => oi.Total).HasPrecision(18, 2);
        }
    }
}
