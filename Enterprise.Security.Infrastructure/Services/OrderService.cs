using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Orders;
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
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _audit;
        private readonly IUserRepository _userRepository; // Descomentar si quieres buscar el nombre del cliente
        private readonly IInvoiceService _invoiceService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICurrentUserService currentUser,
            IAuditService audit,
            IUserRepository userRepository,
            IInvoiceService invoiceService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
            _audit = audit;
            _userRepository = userRepository;
            _invoiceService = invoiceService;
        }

        public async Task<Result<Guid>> CreateOrderAsync(CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return Result<Guid>.Failure("El pedido debe tener al menos un producto.");

            // --- NUEVA LÓGICA: Obtener Nombre del Cliente ---
            var client = await _userRepository.GetByIdAsync(_currentUser.UserId);

            // Si por alguna razón no lo encuentra (raro), ponemos un valor por defecto
            string clientName = client != null
                ? $"{client.FirstName} {client.LastName}"
                : "Cliente Desconocido";
            // -----------------------------------------------

            var order = new Order
            {
                ClientId = _currentUser.UserId,
                ClientName = clientName, // <--- Usamos la variable con el nombre real
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Notes = dto.Notes
            };

            decimal subTotalAcumulado = 0;
            decimal taxAmountAcumulado = 0;

            // --- LÓGICA DE DETALLES ---
            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

                if (product == null)
                    return Result<Guid>.Failure($"Producto con ID {itemDto.ProductId} no encontrado.");

                if (!product.IsActive)
                    return Result<Guid>.Failure($"El producto '{product.Name}' no está disponible.");

                if (product.StockQuantity < itemDto.Quantity)
                    return Result<Guid>.Failure($"Stock insuficiente para '{product.Name}'. Disponible: {product.StockQuantity}");

                // Cálculos
                var lineSubTotal = product.UnitPrice * itemDto.Quantity;
                var lineTax = lineSubTotal * product.TaxRate;

                // Crear Entidad Detalle (Snapshot de precios)
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductSku = product.SKU,
                    ProductName = product.Name,
                    UnitPrice = product.UnitPrice,
                    TaxRate = product.TaxRate,
                    Quantity = itemDto.Quantity,
                    SubTotal = lineSubTotal,
                    TaxAmount = lineTax,
                    Total = lineSubTotal + lineTax
                };

                // Agregamos a la lista en memoria (Aún no se guarda en BD)
                order.Items.Add(orderItem);

                subTotalAcumulado += lineSubTotal;
                taxAmountAcumulado += lineTax;
            }

            // --- TOTALES CABECERA ---
            order.SubTotal = subTotalAcumulado;
            order.TaxAmount = taxAmountAcumulado;
            order.TotalAmount = subTotalAcumulado + taxAmountAcumulado;

            // --- GUARDADO TRANSACCIONAL ---
            // EF Core insertará Order y todos los OrderItems en una sola transacción DB.
            await _orderRepository.AddAsync(order);

            // Audit
            await _audit.LogAsync(
                action: AuditAction.CreateOrder, // Asegúrate de tener este ID en el Enum (ej: 120)
                entity: "Order",
                userId: _currentUser.UserId,
                ipAddress: _currentUser.IpAddress,
                additionalData: $"Creó pedido #{order.Id} con {order.Items.Count} items. Total: {order.TotalAmount:C}"
            );

            return Result<Guid>.Success(order.Id);
        }

        public async Task<Result<List<OrderResponseDto>>> GetMyOrdersAsync()
        {
            var orders = await _orderRepository.GetByClientIdAsync(_currentUser.UserId);
            var dtos = orders.Select(MapToDto).ToList();
            return Result<List<OrderResponseDto>>.Success(dtos);
        }

        public async Task<Result<List<OrderResponseDto>>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var dtos = orders.Select(MapToDto).ToList();
            return Result<List<OrderResponseDto>>.Success(dtos);
        }

        public async Task<Result<OrderResponseDto>> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
                return Result<OrderResponseDto>.Failure("Pedido no encontrado.");

            // Seguridad: Si es cliente, solo puede ver SU pedido
            // (Asumimos que si no es cliente es Admin/Supervisor, validación simple)
            // Puedes mejorar esto verificando Roles si es necesario.
            /* if (_currentUser.UserId != order.ClientId && !UserIsAdmin) 
                 return Result...Failure("No autorizado");
            */

            return Result<OrderResponseDto>.Success(MapToDto(order));
        }

        public async Task<Result<string>> ApproveOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return Result<string>.Failure("Pedido no encontrado.");

            if (order.Status != OrderStatus.Pending)
                return Result<string>.Failure($"El pedido no está en estado Pendiente (Estado actual: {order.Status}).");

            // 1. Validar y Reducir Stock
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);

                if (product == null) return Result<string>.Failure($"Producto {item.ProductSku} ya no existe.");

                if (product.StockQuantity < item.Quantity)
                    return Result<string>.Failure($"Stock insuficiente para {product.Name}. Stock actual: {product.StockQuantity}");

                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // 2. Cambiar Estado
            order.Status = OrderStatus.Approved;
            await _orderRepository.UpdateAsync(order);

            // 3. Generar Factura Automática
            // CAMBIO: Pasamos el ID del usuario actual (Supervisor)
            var invoiceResult = await _invoiceService.GenerateInvoiceFromOrderAsync(order.Id, _currentUser.UserId);

            if (!invoiceResult.IsSuccess)
            {
                return Result<string>.Failure($"Pedido aprobado, pero falló la facturación: {invoiceResult.Error}");
            }

            // 4. Auditoría CORREGIDA
            // Usamos argumentos nombrados (nombre: valor) para evitar que los datos se crucen.
            await _audit.LogAsync(
                action: AuditAction.ApproveOrder,
                entity: "Order",
                userId: _currentUser.UserId,
                // Aquí forzamos el orden correcto usando nombres:
                ipAddress: _currentUser.IpAddress,
                additionalData: $"Aprobó pedido #{order.Id}. Factura generada."
            );

            return Result<string>.Success($"Pedido aprobado y facturado correctamente.");
        }

        // Helper de Mapeo
        private static OrderResponseDto MapToDto(Order o) => new(
            o.Id,
            o.ClientId,
            o.ClientName,
            o.OrderDate,
            o.Status.ToString(),
            o.SubTotal,
            o.TaxAmount,
            o.TotalAmount,
            o.Notes,
            o.Items.Select(i => new OrderItemResponseDto(
                i.ProductId,
                i.ProductSku,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.SubTotal,
                i.TaxAmount,
                i.Total
            )).ToList()
        );
    }
}
