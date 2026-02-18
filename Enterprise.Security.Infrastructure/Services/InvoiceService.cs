using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Invoicing;
using Enterprise.Security.Application.Interfaces.Auditing;
using Enterprise.Security.Application.Interfaces.Persistence;
using Enterprise.Security.Application.Interfaces.Services;
using Enterprise.Security.Domain.Entities;
using Enterprise.Security.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository; // Para venta directa (stock)
        private readonly IUserRepository _userRepository;       // Para nombres
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _audit;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUser,
            IAuditService audit)
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _audit = audit;
        }

        // --- CASO 1: FACTURACIÓN AUTOMÁTICA (Desde Pedido) ---
        public async Task<Result<Guid>> GenerateInvoiceFromOrderAsync(Guid orderId, Guid approverId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return Result<Guid>.Failure("Pedido no encontrado.");

            // Validar que no exista ya una factura para este pedido
            // (Nota: Esto idealmente se hace en DB con índice único, pero validamos aquí también)
            // Por simplicidad, asumimos que el flujo de aprobación controla esto.

            var approver = await _userRepository.GetByIdAsync(approverId);
            string approverName = approver != null
                ? $"{approver.FirstName} {approver.LastName}"
                : "Supervisor";

            // Generar Número de Factura
            int nextNum = await _invoiceRepository.GetNextInvoiceNumberAsync();
            string invoiceNumber = $"INV-{DateTime.Now.Year}-{nextNum:D4}"; // Ej: INV-2026-0005

            var invoice = new Invoice
            {
                ClientId = order.ClientId,
                ClientName = order.ClientName,
                OrderId = order.Id,

                // --- CAMBIO: Guardamos los datos del aprobador ---
                SellerId = approverId,   // Guardamos el ID del supervisor como vendedor
                SellerName = approverName, // Guardamos su nombre (Snapshot)
                                           // -------------------------------------------------

                InvoiceNumber = invoiceNumber,
                IssuedDate = DateTime.UtcNow,
                SubTotal = order.SubTotal,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount
            };

            foreach (var item in order.Items)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    ProductId = item.ProductId,
                    ProductSku = item.ProductSku,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TaxRate = item.TaxRate,
                    SubTotal = item.SubTotal,
                    TaxAmount = item.TaxAmount,
                    Total = item.Total
                });
            }

            await _invoiceRepository.AddAsync(invoice);

            return Result<Guid>.Success(invoice.Id);
        }

        // --- CASO 2: FACTURACIÓN DIRECTA (Vendedor) ---
        public async Task<Result<Guid>> CreateDirectInvoiceAsync(CreateDirectInvoiceDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return Result<Guid>.Failure("La factura debe tener items.");

            // Obtener datos del cliente
            var client = await _userRepository.GetByIdAsync(dto.ClientId);
            string clientName = client != null ? $"{client.FirstName} {client.LastName}" : "Cliente Mostrador";

            // Obtener datos del vendedor (Usuario actual)
            var seller = await _userRepository.GetByIdAsync(_currentUser.UserId);
            string sellerName = seller != null ? $"{seller.FirstName} {seller.LastName}" : "Vendedor";

            // Generar Número
            int nextNum = await _invoiceRepository.GetNextInvoiceNumberAsync();
            string invoiceNumber = $"INV-{DateTime.Now.Year}-{nextNum:D4}";

            var invoice = new Invoice
            {
                ClientId = dto.ClientId,
                ClientName = clientName,
                OrderId = null, // Venta directa
                SellerId = _currentUser.UserId,
                SellerName = sellerName,
                InvoiceNumber = invoiceNumber,
                IssuedDate = DateTime.UtcNow
            };

            decimal subTotalAcc = 0;
            decimal taxAcc = 0;

            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

                if (product == null) return Result<Guid>.Failure($"Producto {itemDto.ProductId} no encontrado.");
                if (!product.IsActive) return Result<Guid>.Failure($"Producto {product.Name} no disponible.");

                // --- VALIDACIÓN Y REDUCCIÓN DE STOCK ---
                if (product.StockQuantity < itemDto.Quantity)
                    return Result<Guid>.Failure($"Stock insuficiente para {product.Name}.");

                product.StockQuantity -= itemDto.Quantity;
                await _productRepository.UpdateAsync(product); // Guardamos el nuevo stock inmediatamente
                                                               // ----------------------------------------

                // Cálculos
                var lineSubTotal = product.UnitPrice * itemDto.Quantity;
                var lineTax = lineSubTotal * product.TaxRate;

                invoice.Items.Add(new InvoiceItem
                {
                    ProductId = product.Id,
                    ProductSku = product.SKU,
                    ProductName = product.Name,
                    UnitPrice = product.UnitPrice,
                    Quantity = itemDto.Quantity,
                    TaxRate = product.TaxRate,
                    SubTotal = lineSubTotal,
                    TaxAmount = lineTax,
                    Total = lineSubTotal + lineTax
                });

                subTotalAcc += lineSubTotal;
                taxAcc += lineTax;
            }

            invoice.SubTotal = subTotalAcc;
            invoice.TaxAmount = taxAcc;
            invoice.TotalAmount = subTotalAcc + taxAcc;

            await _invoiceRepository.AddAsync(invoice);

            // Audit
            await _audit.LogAsync(
                AuditAction.SystemAccess, // Puedes crear uno específico 'CreateDirectInvoice'
                "Invoice",
                _currentUser.UserId,
                _currentUser.IpAddress,
                $"Venta directa {invoiceNumber}. Total: {invoice.TotalAmount:C}"
            );

            return Result<Guid>.Success(invoice.Id);
        }

        // --- Consultas (Igual que en Pedidos) ---
        public async Task<Result<List<InvoiceResponseDto>>> GetMyInvoicesAsync()
        {
            var invoices = await _invoiceRepository.GetByClientIdAsync(_currentUser.UserId);
            return Result<List<InvoiceResponseDto>>.Success(invoices.Select(MapToDto).ToList());
        }

        public async Task<Result<List<InvoiceResponseDto>>> GetAllInvoicesAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return Result<List<InvoiceResponseDto>>.Success(invoices.Select(MapToDto).ToList());
        }

        public async Task<Result<InvoiceResponseDto>> GetByIdAsync(Guid id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null) return Result<InvoiceResponseDto>.Failure("Factura no encontrada");
            return Result<InvoiceResponseDto>.Success(MapToDto(invoice));
        }

        private static InvoiceResponseDto MapToDto(Invoice i) => new(
            i.Id,
            i.InvoiceNumber,
            i.IssuedDate,
            i.ClientName,
            i.SellerName,
            i.OrderId,
            i.SubTotal,
            i.TaxAmount,
            i.TotalAmount,
            i.Items.Select(item => new InvoiceItemDto(
                item.ProductId,
                item.ProductSku,
                item.ProductName,
                item.UnitPrice,
                item.Quantity,
                item.TaxRate,
                item.SubTotal,
                item.TaxAmount,
                item.Total
            )).ToList()
        );
    }
}
