using Microsoft.AspNetCore.Mvc;
using om.security.businesslogic.Interfaces;
using om.security.models;
using om.shared.security;
using System.Threading.Tasks;

namespace om.security.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        protected readonly IAuthBusinessLogic _businessLogic;
        public AuthController(IAuthBusinessLogic businessLogic)
        {
            this._businessLogic = businessLogic;
        }

        [HttpPost("token")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] TokenRequest tokenRequest)
        {

            ValidateCredentialResponse validateCredResponse = await this._businessLogic.AuthenticateAsync(tokenRequest.UserName, tokenRequest.Password);
            if (validateCredResponse.ErrorCode != 0)
            {
                return Unauthorized();
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
            return Unauthorized();
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await this._businessLogic.Logout(this.GetLoggedInUserId<string>());
            return Ok();
        }
    }
}
