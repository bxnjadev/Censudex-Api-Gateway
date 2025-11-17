using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_api_gateway.src.Dtos
{
    /// <summary>
    /// LogoutDto
    /// </summary>
    public class LogoutDto
    {
        public string Token { get; set; } = string.Empty;
    }
}