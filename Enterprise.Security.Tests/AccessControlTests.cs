using Enterprise.Security.Api.Controllers;
using Enterprise.Security.Application.Common;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Enterprise.Security.Tests
{
    public class AccessControlTests
    {
        // Rúbrica Punto 5: Restricción de acceso por rol (Permisos)

        [Theory]
        [InlineData(typeof(InvoicesController), "CreateDirect", Permissions.Invoices.CreateDirect)]
        [InlineData(typeof(InvoicesController), "GetAll", Permissions.Invoices.ViewAll)]
        [InlineData(typeof(OrdersController), "Approve", Permissions.Orders.Approve)]
        [InlineData(typeof(OrdersController), "Create", Permissions.Orders.Create)]
        public void Endpoint_Should_Have_Correct_Permission_Attribute(Type controllerType, string methodName, string expectedPermission)
        {
            // 1. Buscar el método en el controlador
            var methodInfo = controllerType.GetMethod(methodName);
            Assert.NotNull(methodInfo);

            // 2. Obtener el atributo [Authorize]
            var authorizeAttribute = methodInfo.GetCustomAttribute<AuthorizeAttribute>();

            // 3. Verificar que existe y tiene el permiso correcto (Policy)
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(expectedPermission, authorizeAttribute.Policy);
        }

        [Fact]
        public void InvoicesController_Should_Be_Protected_By_Default()
        {
            // Verificar que la CLASE del controlador tiene [Authorize] o hereda seguridad
            // En tu caso, usaste [Authorize] en cada método, lo cual es válido, 
            // pero verificamos al menos uno clave.

            var method = typeof(InvoicesController).GetMethod("GetMyInvoices");
            var attr = method?.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attr);
            Assert.Equal(Permissions.Invoices.ViewMy, attr.Policy);
        }
    }
}
