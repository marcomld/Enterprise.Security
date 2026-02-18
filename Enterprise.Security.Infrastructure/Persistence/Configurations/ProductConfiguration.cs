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
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            // SKU debe ser único
            builder.HasIndex(p => p.SKU)
                .IsUnique();

            builder.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Configuración de decimales para dinero (18 dígitos, 2 decimales)
            builder.Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(p => p.TaxRate)
                .HasPrecision(18, 2);

            // Relación
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrar una categoría si tiene productos
        }
    }
}
