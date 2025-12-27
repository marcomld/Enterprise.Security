using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Infrastructure.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings"; // Ayuda para mapear la configuración desde appsettings.json
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public int ExpirationMinutes { get; set; }
    }
}
