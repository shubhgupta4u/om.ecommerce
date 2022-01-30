using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Security.Claims;

namespace om.shared.security.Interfaces
{
    public interface IAuthService
    {
        void RegisterAuthentication(IServiceCollection service);
        bool ValidateToken(string authToken, out IEnumerable<Claim> claims);
        bool ValidateToken(string authToken, string allowedRoles, out int errorCode);
    }
}
