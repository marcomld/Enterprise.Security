using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Persistence.DbContext
{
    public class SecurityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
        {
        }

        // --- TUS TABLAS (Ahora solo las esenciales) ---
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        // UserSessions eliminada por solicitud
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // =========================================================
            // 1. ZONA DE LIMPIEZA: Ignorar TODO lo que no es esencial
            // =========================================================

            builder.Ignore<IdentityUserClaim<Guid>>();
            builder.Ignore<IdentityRoleClaim<Guid>>();
            builder.Ignore<IdentityUserLogin<Guid>>();
            builder.Ignore<IdentityUserToken<Guid>>();

            // Ignoramos UserSessions o RefreshTokens si había configuración previa de Identity
            // (Al no ponerla en el DbSet, EF Core la ignora por defecto, 
            //  pero si tienes configuración en un archivo separado, asegúrate de borrar ese archivo también).

            // =========================================================
            // 2. RENOMBRAMIENTO: Solo las tablas núcleo
            // =========================================================

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
            });

            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(r => new { r.UserId, r.RoleId });
            });

            // =========================================================
            // 3. CONFIGURACIÓN DE TUS TABLAS DE NEGOCIO
            // =========================================================

            builder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");
            });

            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            });
        }
    }
}
