using Microsoft.AspNetCore.Mvc;
using om.security.businesslogic.Interfaces;
using om.security.models;
using om.shared.logger.Interfaces;
using om.shared.security;
using System.Threading.Tasks;

namespace om.security.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        protected readonly IAuthBusinessLogic _businessLogic;
        protected readonly ILogger _logger;
        public AuthController(IAuthBusinessLogic businessLogic, ILogger logger)
        {
            this._businessLogic = businessLogic;
            this._logger = logger;
        }

        [HttpPost("token")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] TokenRequest tokenRequest)
        {
            ValidateCredentialResponse validateCredResponse = null;
            if(tokenRequest.GrantType == GrantType.Okta)
            {
                validateCredResponse = await this._businessLogic.AuthenticateAsync(tokenRequest.BearerToken, GrantType.Okta);
            }
            else if (tokenRequest.GrantType == GrantType.AzureAD)
            {
                validateCredResponse = await this._businessLogic.AuthenticateAsync(tokenRequest.BearerToken, GrantType.AzureAD);
            }
            else
            {
                validateCredResponse = await this._businessLogic.AuthenticateAsync(tokenRequest.UserName, tokenRequest.Password);
            }

            if (validateCredResponse == null || validateCredResponse.ErrorCode != 0)
            {
                return Unauthorized("You have entered an invalid username or password.");
            }
            TokenResponse response = await this._businessLogic.GenerateTokenAsync(validateCredResponse, this.GetRemoteIpAddress()); ;
            return Ok(response);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenResponse tokenResponse)
        {
            TokenResponse response = await this._businessLogic.GenerateRefreshTokenAsync(tokenResponse,this.GetRemoteIpAddress());
            if (response!=null)
            {
                return Ok(response);                
            }
            return Unauthorized("You have provided an invalid bearer jwt or refresh tokens.");
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await this._businessLogic.Logout(this.GetLoggedInUserId<string>());
            return Ok();
        }
    }
}
