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
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(x => x.Id);

            // Conversión explícita de Enum a int (aunque es default, es bueno ser explícito)
            builder.Property(x => x.Action)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.Entity).HasMaxLength(100).IsRequired();
            builder.Property(x => x.IpAddress).HasMaxLength(45); // IPv6 max length

            // Indexar por UserId para búsquedas rápidas de auditoría
            builder.HasIndex(x => x.UserId);
        }

    }
}
