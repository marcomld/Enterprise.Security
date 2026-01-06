using Enterprise.Security.Application.Common; // Asegúrate de tener este using para los Permisos
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
                // 1. Aplicar migraciones
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.MigrateAsync();
                }

                // 2. Ejecutar Seeders
                await SeedRolesAsync();
                await SeedPermissionsAsync();
                await SeedRolePermissionsAsync();
                await SeedUsersAsync(); // Renombrado para incluir múltiples usuarios
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
                throw;
            }
        }

        // --- SEEDERS ---

        private async Task SeedRolesAsync()
        {
            // Agregamos un rol "Manager" para pruebas intermedias
            var roles = new[] { "Admin", "Manager", "User" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(roleName)
                    {
                        Description = roleName == "Admin" ? "Control Total" :
                                      roleName == "Manager" ? "Gestión de Usuarios" : "Solo Lectura",
                        IsSystemRole = true
                    });
                    _logger.LogInformation($"Rol creado: {roleName}");
                }
            }
        }

        private async Task SeedPermissionsAsync()
        {
            // Reflection para obtener todos los permisos definidos en el código
            var permissionClass = typeof(Permissions);
            var modules = permissionClass.GetNestedTypes();

            foreach (var module in modules)
            {
                var moduleName = module.Name;
                var fields = module.GetFields(System.Reflection.BindingFlags.Public |
                                              System.Reflection.BindingFlags.Static |
                                              System.Reflection.BindingFlags.FlattenHierarchy);

                foreach (var field in fields)
                {
                    var permissionCode = (string)field.GetValue(null)!;
                    var description = $"Permiso para {permissionCode}";

                    if (!await _context.Permissions.AnyAsync(p => p.Code == permissionCode))
                    {
                        var permission = new Permission(permissionCode, description, moduleName);
                        _context.Permissions.Add(permission);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedRolePermissionsAsync()
        {
            var allPermissions = await _context.Permissions.ToListAsync();

            // 1. ADMIN: Tiene TODOS los permisos
            await AssignPermissionsToRole("Admin", allPermissions.Select(p => p.Code).ToList());

            // 2. MANAGER: Tiene permisos de Usuarios (Menos Borrar) y Solo ver Roles
            var managerPermissions = new List<string>
            {
                Permissions.Users.View,
                Permissions.Users.Create,
                Permissions.Users.Edit,
                // No puede borrar usuarios (Users.Delete)
                
                Permissions.Roles.View
                // No puede gestionar roles (Roles.Manage, Roles.Assign)
            };
            await AssignPermissionsToRole("Manager", managerPermissions);

            // 3. USER: Solo puede VER usuarios (Directorio)
            var userPermissions = new List<string>
            {
                Permissions.Users.View
            };
            await AssignPermissionsToRole("User", userPermissions);
        }

        // Helper para asignar lista de permisos a un rol
        private async Task AssignPermissionsToRole(string roleName, List<string> permissionCodes)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return;

            var permissionsEntities = await _context.Permissions
                .Where(p => permissionCodes.Contains(p.Code))
                .ToListAsync();

            var existingRelations = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToListAsync();

            foreach (var permission in permissionsEntities)
            {
                if (!existingRelations.Any(rp => rp.PermissionId == permission.Id))
                {
                    _context.RolePermissions.Add(new RolePermission(role.Id, permission.Id));
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedUsersAsync()
        {
            // 1. Crear ADMIN
            await CreateUser("admin@enterprise.com", "System", "Administrator", "Admin");

            // 2. Crear MANAGER (Prueba de permisos intermedios)
            await CreateUser("manager@enterprise.com", "Roberto", "Gerente", "Manager");

            // 3. Crear USER (Prueba de solo lectura)
            await CreateUser("user@enterprise.com", "Laura", "Usuario", "User");
        }

        private async Task CreateUser(string email, string firstName, string lastName, string role)
        {
            const string password = "Password123!";

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = role.ToLower(), // ej: 'admin', 'manager' para login fácil
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString() // Importante para la validación de seguridad
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    _logger.LogInformation($"Usuario {role} ({email}) creado.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Error creando {role}: {errors}");
                }
            }
        }
    }
}