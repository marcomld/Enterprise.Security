using Enterprise.Security.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public string Code { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public string Module { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;

        private Permission() { }

        public Permission(string code, string description, string module)
        {
            Code = code;
            Description = description;
            Module = module;
        }
    }
}
