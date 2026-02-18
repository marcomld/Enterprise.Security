using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Domain.Enums;
using Enterprise.Security.Infrastructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Security.Tests
{
    public class OrderServiceTests
    {
        // Definimos los Mocks (Objetos falsos simulados)
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<IAuditService> _mockAudit;
        private readonly Mock<IInvoiceService> _mockInvoiceService;

        // El servicio real que vamos a probar
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            // 1. Inicializamos los simuladores
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockAudit = new Mock<IAuditService>();
            _mockInvoiceService = new Mock<IInvoiceService>();

            // 2. Configuramos comportamiento básico (quién es el usuario actual)
            _mockCurrentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

            // 3. Inyectamos los mocks en el servicio real
            _orderService = new OrderService(
                _mockOrderRepo.Object,
                _mockProductRepo.Object,
                _mockCurrentUser.Object,
                _mockAudit.Object,
                _mockUserRepo.Object, // Asegúrate de que el orden coincida con tu constructor
                _mockInvoiceService.Object
            );
        }

        [Fact]
        public async Task ApproveOrder_Should_ReduceStock_And_GenerateInvoice_When_Valid()
        {
            // --- ARRANGE (Preparar el escenario) ---
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var supervisorId = Guid.NewGuid();

            // Simulamos un producto con Stock de 10
            var product = new Product
            {
                Id = productId,
                Name = "Laptop Gamer",
                StockQuantity = 10,
                IsActive = true
            };

            // Simulamos un pedido Pendiente con 1 item de cantidad 2
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                Items = new List<OrderItem>
            {
                new OrderItem { ProductId = productId, Quantity = 2, ProductName = "Laptop Gamer" }
            }
            };

            // Configuramos los repositorios para que devuelvan nuestros datos falsos
            _mockOrderRepo.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockProductRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);

            // Simulamos que la facturación es exitosa
            _mockInvoiceService.Setup(x => x.GenerateInvoiceFromOrderAsync(orderId, It.IsAny<Guid>()))
                               .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

            // --- ACT (Ejecutar la acción) ---
            var result = await _orderService.ApproveOrderAsync(orderId);

            // --- ASSERT (Verificar resultados) ---

            // 1. Debe ser exitoso
            Assert.True(result.IsSuccess);

            // 2. El estado del pedido debe cambiar a Approved
            Assert.Equal(OrderStatus.Approved, order.Status);

            // 3. El stock debe bajar de 10 a 8 (10 - 2)
            Assert.Equal(8, product.StockQuantity);

            // 4. Se debió llamar al repositorio para actualizar el producto
            _mockProductRepo.Verify(x => x.UpdateAsync(product), Times.Once);

            // 5. Se debió llamar al repositorio para actualizar el pedido
            _mockOrderRepo.Verify(x => x.UpdateAsync(order), Times.Once);

            // 6. ¡CRÍTICO! Se debió llamar al servicio de facturación
            _mockInvoiceService.Verify(x => x.GenerateInvoiceFromOrderAsync(orderId, It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task ApproveOrder_Should_ReturnFailure_And_NotCallInvoice_When_StatusIsNotPending()
        {
            // --- ARRANGE ---
            var orderId = Guid.NewGuid();

            // Simulamos un pedido que YA está aprobado (Estado inválido para aprobar de nuevo)
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Approved, // Ya aprobado
                Items = new List<OrderItem>()
            };

            _mockOrderRepo.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

            // --- ACT ---
            var result = await _orderService.ApproveOrderAsync(orderId);

            // --- ASSERT ---

            // 1. Debe fallar
            Assert.False(result.IsSuccess);
            Assert.Contains("no está en estado Pendiente", result.Error);

            // 2. CRÍTICO (Rúbrica Punto 2): Verificar que NUNCA se intentó generar factura
            _mockInvoiceService.Verify(
                x => x.GenerateInvoiceFromOrderAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Never // <-- Esto asegura el cumplimiento
            );
        }
    }
}
