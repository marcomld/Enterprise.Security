using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public bool IsSystemRole { get; private set; }

        private Role() { }

        public Role(string name, string description, bool isSystemRole = false)
        {
            Name = name;
            Description = description;
            IsSystemRole = isSystemRole;
        }
    }
}
