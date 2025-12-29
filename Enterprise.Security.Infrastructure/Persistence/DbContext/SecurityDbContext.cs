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

        // Tablas del Dominio Puro
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        // Eliminamos RefreshTokens porque ya son columnas dentro de Users
        //public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        // Agregamos Permissions si los usaas en el dominio
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Aplicar configuraciones externas (Busca las clases IEntityTypeConfiguration en este proyecto)
            // Esto cargará automáticamente AuditLogConfiguration
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // 2. Renombrar tablas de Identity (Limpieza Enterprise)
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

            // 3. Configuraciones rápidas (si no creaste archivo separado)
            builder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Token).IsUnique();
            });

            builder.Entity<UserSession>(entity =>
            {
                entity.ToTable("UserSessions");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.UserId);
            });

            // Configuración para RolePermission (Muchos a Muchos manual)
            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            });
        }
    }
}
