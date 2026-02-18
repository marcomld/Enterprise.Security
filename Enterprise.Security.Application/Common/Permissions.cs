using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Common
{
    public static class Permissions
    {
        // Módulo de Usuarios
        public static class Users
        {
            public const string View = "users.view";
            public const string Create = "users.create";
            public const string Edit = "users.edit";
            public const string Delete = "users.delete";
        }

        // Módulo de Roles
        public static class Roles
        {
            public const string View = "roles.view";
            public const string Manage = "roles.manage"; // Crear, Asignar, Borrar roles
            public const string Assign = "roles.assign";
        }

        // Módulo de Auditoría
        public static class Audits
        {
            public const string View = "audits.view";
            // Generalmente un auditor solo ve, no edita ni borra logs (por seguridad)
            // public const string Export = "audits.export"; // Podrías agregar este a futuro
        }

        public static class Categories
        {
            public const string View = "Permissions.Categories.View";
            public const string Create = "Permissions.Categories.Create";
            public const string Edit = "Permissions.Categories.Edit";
            public const string Delete = "Permissions.Categories.Delete";
        }

        public static class Orders
        {
            public const string ViewAll = "Permissions.Orders.ViewAll"; // Supervisor
            public const string ViewMy = "Permissions.Orders.ViewMy";   // Cliente
            public const string Create = "Permissions.Orders.Create";   // Cliente
            public const string Approve = "Permissions.Orders.Approve"; // Supervisor
        }

        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string Create = "Permissions.Products.Create";
            public const string Edit = "Permissions.Products.Edit";
            public const string Delete = "Permissions.Products.Delete";
        }

        public static class Invoices
        {
            public const string ViewAll = "Permissions.Invoices.ViewAll";       // Supervisor
            public const string ViewMy = "Permissions.Invoices.ViewMy";         // Cliente
            public const string CreateDirect = "Permissions.Invoices.CreateDirect"; // Vendedor (Venta Directa)
                                                                                    // Nota: No hay "CreateFromOrder" porque eso lo hace el sistema automáticamente al aprobar.
        }

        // Módulo de Permisos (Para que alguien pueda asignar permisos a roles)
        public static class SystemPermissions
        {
            public const string Manage = "permissions.manage";
        }

        /// <summary>
        /// Este método mágico busca todas las constantes dentro de las clases anidadas.
        /// Sirve para registrar las Policies automáticamente y para el Seed.
        /// </summary>
        public static List<string> GetAll()
        {
            var permissions = new List<string>();

            // Obtenemos todas las clases anidadas (Users, Roles, etc.)
            var nestedClasses = typeof(Permissions).GetNestedTypes(BindingFlags.Public);

            foreach (var nestedClass in nestedClasses)
            {
                // Obtenemos todos los campos constantes de cada clase
                var fields = nestedClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));

                foreach (var field in fields)
                {
                    var value = field.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        permissions.Add(value);
                    }
                }
            }

            return permissions;
        }
    }
}
