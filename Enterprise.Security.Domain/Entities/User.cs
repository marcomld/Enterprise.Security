using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public DateTime? LastLoginDate { get; private set; }

        private User() { }

        public User(string username, string email)
        {
            Username = username;
            Email = email;
        }

        public void Desactivate()
        {
            IsActive = false;
        }
        public void Activate()
        {
            IsActive = true;
        }
        public void RegisterLogin()
        {
            LastLoginDate = DateTime.UtcNow;
        }

        // Agregar esto para soportar la relación a nivel de objeto
        private readonly List<UserRole> _userRoles = new();
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    }
}
