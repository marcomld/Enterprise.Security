using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Enums
{
    public enum AuditAction
    {
        // --- Authentication Actions (1-9) ---
        Login = 1,
        Logout = 2,
        FailedLogin = 3,
        TokenRefreshed = 4,
        UnauthorizedAccess = 5, // Importante: Usuario logueado intenta entrar donde no debe

        // --- User Management Actions (10-19) ---
        UserCreated = 10,
        UserUpdated = 11,
        UserDeactivated = 12, // Corregido (antes Desactivated)
        UserActivated = 13,
        PasswordChanged = 14,
        PasswordResetRequested = 15,
        PasswordResetCompleted = 16,
        UserDeleted = 17,     // Agregado por si implementas borrado (soft/hard)

        // --- Roles and Permissions Actions (20-39) ---
        RoleCreated = 20,
        RoleUpdated = 21,
        RoleDeleted = 22,
        RoleAssignedToUser = 23,     // Corregido (antes RoleAssignedToRole)
        RoleRemovedFromUser = 24,

        PermissionCreated = 30,      // (antes PermissionGranted, para ser consistente con RoleCreated)
        PermissionAssignedToRole = 31,
        PermissionRevokedFromRole = 32,

        // --- Security (40-89) ---
        AccountLocked = 40,
        AccountUnlocked = 41,
        TwoFactorEnabled = 42,
        TwoFactorDisabled = 43,
        InvalidRefreshTokenUsed = 44, // Importante: Detectar posible robo de tokens

        // --- System Events (90-99) ---
        SystemAccess = 90,
        SystemError = 91,

        // Inventory - Categories (100-109)
        CreateCategory = 100,
        UpdateCategory = 101,
        DeleteCategory = 102,

        // Inventory - Products (110-119)
        CreateProduct = 110,
        UpdateProduct = 111,
        DeleteProduct = 112,
        AdjustStock = 113,

        // Orders (Reserva para futuro)
        CreateOrder = 120,
        ApproveOrder = 121
    }
}
