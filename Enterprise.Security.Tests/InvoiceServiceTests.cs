using Enterprise.Security.Application.DTOs.Invoicing;
using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Domain.Common;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Infrastructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Security.Tests
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IInvoiceRepository> _mockInvoiceRepo;
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<IAuditService> _mockAudit;

        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _mockInvoiceRepo = new Mock<IInvoiceRepository>();
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockAudit = new Mock<IAuditService>();

            _invoiceService = new InvoiceService(
                _mockInvoiceRepo.Object,
                _mockOrderRepo.Object,
                _mockProductRepo.Object,
                _mockUserRepo.Object,
                _mockCurrentUser.Object,
                _mockAudit.Object
            );
        }

        // --- Rúbrica Punto 1, 4 y 6: Generación Automática, Asociación e Integridad ---
        [Fact]
        public async Task GenerateInvoiceFromOrder_Should_CreateCorrectInvoice()
        {
            // ARRANGE
            var orderId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var approverId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Datos Mock del Pedido
            var order = new Order
            {
                Id = orderId,
                ClientId = clientId,
                ClientName = "Juan Perez",
                SubTotal = 100,
                TaxAmount = 15,
                TotalAmount = 115,
                Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = productId,
                    ProductName = "Mouse",
                    Quantity = 2,
                    UnitPrice = 50,
                    Total = 115,
                    ProductSku = "SKU-123" // Agregamos SKU para evitar nulos
                }
            }
            };

            // Mock del Supervisor (Aprobador)
            // CORRECCIÓN: Usamos la entidad de Dominio 'User' creada con el helper
            var approverUser = CreateTestUser(approverId, "Admin", "User");

            _mockOrderRepo.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockInvoiceRepo.Setup(x => x.GetNextInvoiceNumberAsync()).ReturnsAsync(1);

            // Aquí estaba el error: Ahora devolvemos 'User' (Domain), no 'ApplicationUser' (Infrastructure)
            _mockUserRepo.Setup(x => x.GetByIdAsync(approverId)).ReturnsAsync(approverUser);

            // ACT
            var result = await _invoiceService.GenerateInvoiceFromOrderAsync(orderId, approverId);

            // ASSERT
            Assert.True(result.IsSuccess);

            _mockInvoiceRepo.Verify(x => x.AddAsync(It.Is<Invoice>(i =>
                i.OrderId == orderId &&
                i.ClientId == clientId &&
                i.ClientName == "Juan Perez" &&
                i.TotalAmount == 115 &&
                i.Items.Count == 1 &&
                i.SellerId == approverId &&
                i.SellerName == "Admin User" // Verificamos que concatenó bien el nombre
            )), Times.Once);
        }

        // --- Rúbrica Punto 3: Facturación Directa ---
        [Fact]
        public async Task CreateDirectInvoice_Should_ReduceStock_And_CreateInvoice()
        {
            // ARRANGE
            var productId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var sellerId = Guid.NewGuid(); // ID del vendedor actual

            var dto = new CreateDirectInvoiceDto(
                ClientId: clientId,
                Items: new List<CreateDirectInvoiceItemDto>
                {
                new CreateDirectInvoiceItemDto(productId, 5)
                }
            );

            var product = new Product
            {
                Id = productId,
                Name = "Teclado",
                StockQuantity = 20,
                UnitPrice = 10,
                IsActive = true,
                SKU = "KEY-001"
            };

            // Mock del Cliente y Vendedor
            var clientUser = CreateTestUser(clientId, "Cliente", "Test");
            var sellerUser = CreateTestUser(sellerId, "Vendedor", "Test");

            _mockProductRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockInvoiceRepo.Setup(x => x.GetNextInvoiceNumberAsync()).ReturnsAsync(2);
            _mockCurrentUser.Setup(x => x.UserId).Returns(sellerId);

            // Mocks de usuarios para nombres
            _mockUserRepo.Setup(x => x.GetByIdAsync(clientId)).ReturnsAsync(clientUser);
            _mockUserRepo.Setup(x => x.GetByIdAsync(sellerId)).ReturnsAsync(sellerUser);

            // ACT
            var result = await _invoiceService.CreateDirectInvoiceAsync(dto);

            // ASSERT
            Assert.True(result.IsSuccess);

            // 1. Verificar reducción de Stock
            Assert.Equal(15, product.StockQuantity);
            _mockProductRepo.Verify(x => x.UpdateAsync(product), Times.Once);

            // 2. Verificar creación de factura
            _mockInvoiceRepo.Verify(x => x.AddAsync(It.Is<Invoice>(i =>
                i.OrderId == null &&
                i.ClientId == clientId &&
                i.ClientName == "Cliente Test" &&
                i.SellerId == sellerId &&
                i.SellerName == "Vendedor Test"
            )), Times.Once);
        }

        // --- HELPER PARA CREAR USUARIOS CON SETTERS PRIVADOS ---
        private User CreateTestUser(Guid id, string firstName, string lastName)
        {
            // Usar el método de factoría estático recomendado por la clase User
            return User.LoadExisting(
                id,
                firstName,
                lastName,
                "test@test.com",
                "testuser",
                true,
                null
            );
        }
    }
}
