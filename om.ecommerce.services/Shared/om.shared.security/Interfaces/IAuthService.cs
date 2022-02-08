using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace om.shared.security.Interfaces
{
    public interface IAuthService
    {
        void RegisterAuthentication(IServiceCollection service);
        bool ValidateToken(string authToken, out IEnumerable<Claim> claims);
        bool ValidateToken(string authToken, string allowedRoles, out int errorCode);
        bool ValidateOktaToken(string jwt, out string email);
        bool ValidateMsalToken(string jwt, out string email);
        Task<string> GenerateApiServiceTokenAsync(string serviceAppName, DateTime tokenExpiry);
        Task<string> GenerateRefreshTokenAsync(string userId, string token, string remoteIpAddress);
        TokenValidationParameters GetTokenValidationParameters();
    }
}
