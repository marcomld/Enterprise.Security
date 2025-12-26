using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Enums
{
    public enum AuditAction
    {
        //Autentication Actions
        Login = 1,
        Logout = 2,
        FailedLogin = 3,
        TokenRefreshed = 4,

        //User Management Actions
        UserCreated = 10,
        UserUpdated = 11,
        UserDesactivated = 12,
        UserActivated = 13,
        PasswordChanged = 14,
        PasswordResetRequested = 15,
        PasswordResetCompleted = 16,

        //Roles and Permissions Actions
        RoleCreated = 20,
        RoleUpdated = 21,
        RoleDeleted = 22,
        RoleAssignedToRole = 23,
        RoleRemovedFromUser = 24,

        PermissionGranted = 30,
        PermissionAssignedToRole = 31,
        PermissionRevokedFromRole = 32,

        //Security
        AccountLocked = 40,
        AccountUnlocked = 41,
        TwoFactorEnabled = 42,
        TwoFactorDisabled = 43,

        //System Events
        SystemAccess = 90,
        SystemError = 91

    }
}
