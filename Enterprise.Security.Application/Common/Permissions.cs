using System;
using System.Collections.Generic;
using System.Linq;
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

        // Módulo de Roles (Ejemplo)
        public static class Roles
        {
            public const string View = "roles.view";
            public const string Manage = "roles.manage";
        }

        // Aquí irás agregando todos los módulos de tu sistema
    }
}
