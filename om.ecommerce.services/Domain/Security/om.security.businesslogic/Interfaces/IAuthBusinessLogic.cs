using om.security.models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace om.security.businesslogic.Interfaces
{
    public interface IAuthBusinessLogic
    {
        Task<ValidateCredentialResponse> AuthenticateAsync(string userName, string password);
        Task<ValidateCredentialResponse> AuthenticateAsync(string bearer_token, GrantType grantType);
        Task<TokenResponse> GenerateTokenAsync(ValidateCredentialResponse validateCredResponse, string remoteIpAddress);
        Task<TokenResponse> GenerateRefreshTokenAsync(TokenResponse tokenResponse, string remoteIpAddress);
        Task Logout(string userId);
    }
}
