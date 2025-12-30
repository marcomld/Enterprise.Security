using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Infrastructure.Identity;
using Enterprise.Security.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Persistence.Seed
{
    public class DbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;
        private readonly SecurityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DbInitializer(
            ILogger<DbInitializer> logger,
            SecurityDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task RunAsync()
        {
            try
            {
                // 1. Aplicar migraciones pendientes automáticamente (Opcional, pero recomendado en Dev)
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.MigrateAsync();
                }

                // 2. Ejecutar Seeders en orden
                await SeedRolesAsync();
                await SeedPermissionsAsync();       // Ahora usa Reflection inteligente
                await SeedRolePermissionsAsync();   // Asigna todo al Admin
                await SeedAdminUserAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
                throw;
            }
        }

        // --- SEEDERS INDIVIDUALES ABAJO ---

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "User" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(roleName)
                    {
                        Description = $"Rol por defecto {roleName}",
                        IsSystemRole = true
                    });
                    _logger.LogInformation($"Rol creado: {roleName}");
                }
            }
        }

        private async Task SeedPermissionsAsync()
        {
            // TRUCO ENTERPRISE: Reflection 🧠
            // Obtenemos todas las constantes de tu clase 'Permissions' en Application
            var permissionClass = typeof(Application.Common.Permissions);

            // Buscamos las clases anidadas (Users, Roles, etc.)
            var modules = permissionClass.GetNestedTypes();

            foreach (var module in modules)
            {
                var moduleName = module.Name; // Ej: "Users"

                // Obtenemos los campos constantes (los permisos strings)
                var fields = module.GetFields(System.Reflection.BindingFlags.Public |
                                              System.Reflection.BindingFlags.Static |
                                              System.Reflection.BindingFlags.FlattenHierarchy);

                foreach (var field in fields)
                {
                    var permissionCode = (string)field.GetValue(null)!; // Ej: "users.create"
                    var description = $"Permiso para {permissionCode}"; // Generamos descripción genérica

                    // Verificamos si ya existe en BD para no duplicar
                    if (!await _context.Permissions.AnyAsync(p => p.Code == permissionCode))
                    {
                        // Usamos TU constructor
                        var permission = new Permission(permissionCode, description, moduleName);
                        _context.Permissions.Add(permission);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedRolePermissionsAsync()
        {
            // Buscamos el rol Admin
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole == null) return;

            // Obtenemos todos los permisos guardados en BD
            var allPermissions = await _context.Permissions.ToListAsync();

            // Obtenemos los permisos que el Admin YA tiene asignados
            var existingRelations = await _context.RolePermissions
                .Where(rp => rp.RoleId == adminRole.Id)
                .ToListAsync();

            foreach (var permission in allPermissions)
            {
                // Si el Admin no tiene este permiso, se lo asignamos
                if (!existingRelations.Any(rp => rp.PermissionId == permission.Id))
                {
                    // Usamos TU constructor de RolePermission
                    var relation = new RolePermission(adminRole.Id, permission.Id);
                    _context.RolePermissions.Add(relation);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@enterprise.com";
            const string adminPassword = "Password123!"; // ¡Cambiar en prod!

            var user = await _userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "admin", // O el email, según tu config
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    _logger.LogInformation("Usuario Admin creado exitosamente.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Error creando Admin: {errors}");
                }
            }
        }
    }
}
