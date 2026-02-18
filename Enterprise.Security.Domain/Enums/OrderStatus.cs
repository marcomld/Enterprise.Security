using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,   // Creado por el cliente, esperando revisión
        Approved = 2,  // Aprobado por Supervisor (Generará factura)
        Rejected = 3,  // Rechazado por Supervisor
        Cancelled = 4  // Cancelado por el Cliente antes de aprobación
    }
}
