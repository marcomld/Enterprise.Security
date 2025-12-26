using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Authentication
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(
            Guid userId,
            string userName,
            IEnumerable<string> roles,
            IEnumerable<string> permissions);
    }
}
