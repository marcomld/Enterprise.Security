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
        public string FirstName { get; private set; } = default!; // Nuevo
        public string LastName { get; private set; } = default!;  // Nuevo
        public string Username { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public DateTime? LastLoginAt { get; private set; }

        private User() { }

        // Constructor para NUEVOS usuarios
        public User(string firstName, string lastName, string email, string username)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Username = username;
        }

        // Factory para CARGAR usuarios existentes
        public static User LoadExisting(Guid id, string firstName, string lastName, string email, string username, bool isActive, DateTime? lastLoginAt)
        {
            return new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Username = username,
                IsActive = isActive,
                LastLoginAt = lastLoginAt
            };
        }

        // Método de Negocio para actualizar perfil
        public void UpdateProfile(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("El nombre es requerido.");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("El apellido es requerido.");

            FirstName = firstName;
            LastName = lastName;
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
            LastLoginAt = DateTime.UtcNow;
        }

        // Agregar esto para soportar la relación a nivel de objeto
        private readonly List<UserRole> _userRoles = new();
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    }
}
