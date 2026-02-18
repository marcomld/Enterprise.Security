using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Invoicing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        // Caso 1: Facturación Automática (Desde Pedido)
        Task<Result<Guid>> GenerateInvoiceFromOrderAsync(Guid orderId);

        // Caso 2: Facturación Directa (Vendedor)
        Task<Result<Guid>> CreateDirectInvoiceAsync(CreateDirectInvoiceDto dto);

        // Consultas
        Task<Result<InvoiceResponseDto>> GetByIdAsync(Guid id);
        Task<Result<List<InvoiceResponseDto>>> GetMyInvoicesAsync();
        Task<Result<List<InvoiceResponseDto>>> GetAllInvoicesAsync();
    }
}
