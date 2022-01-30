using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using om.shared.security.Interfaces;
using om.shared.security.models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace om.shared.security
{
    public class AuthService: IAuthService
    {
        private readonly JwtSetting _jwtSetting;
        private readonly IUserTokenRepository _userTokenRepository;
        public AuthService(IOptions<JwtSetting> options, IUserTokenRepository userTokenRepository) {
            this._userTokenRepository = userTokenRepository;
            this._jwtSetting = options.Value;
        }

        public void RegisterAuthentication(IServiceCollection service)
        {
            service.AddAuthentication(item =>
            {
                item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(item =>
                {
                    item.RequireHttpsMetadata = true;
                    item.SaveToken = true;
                    item.TokenValidationParameters = this.GetTokenValidationParameters();
                });
        }
        public bool ValidateToken(string authToken, out IEnumerable<Claim> claims)
        {
            bool isValid = false;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();                
                tokenHandler.ValidateToken(authToken,this.GetTokenValidationParameters(), out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                claims = jwtToken.Claims;
                if (jwtToken != null && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256) && claims.Any())
                {
                    UserTokenInfo userTokenInfo=this._userTokenRepository.GetAsync(claims.FirstOrDefault(x => x.Type == "nameid").Value).Result;
                    if (userTokenInfo != null && userTokenInfo.IsActive 
                            && (userTokenInfo.CurrentToken == authToken 
                                || (userTokenInfo.PreviousToken == authToken && userTokenInfo.ModifiedDate.AddMinutes(this._jwtSetting.PreviousTokenValidDuration) > DateTimeOffset.UtcNow)
                               )
                        )
                    {
                        isValid = true;
                    }                    
                }
            }
            catch(Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException ex)
            {
                claims = null;
            }
            catch
            {
                claims = null;
            }
            return isValid;
        }
        public bool ValidateToken(string authToken,string allowedRoles, out int errorCode)
        {
            errorCode = 401;
            IEnumerable<Claim> claims;
            bool isValid = this.ValidateToken(authToken, out claims);
            if (isValid)
            {
                var userClaim = claims.FirstOrDefault(x => x.Type == "nameid");
                var roleClaims = claims.Where(x => x.Type == "role");
                if (userClaim != null && !string.IsNullOrWhiteSpace(userClaim.Value) && allowedRoles == null)
                {
                    errorCode = 0;
                }
                else if (userClaim != null && !string.IsNullOrWhiteSpace(userClaim.Value) && allowedRoles != null)
                {
                    var roles = allowedRoles.Split(",");
                    errorCode = 403;
                    isValid = false;
                    foreach (Claim claim in roleClaims)
                    {
                        if (roles.Any(s => s.Equals(claim.Value, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            errorCode = 0;
                            isValid = true;
                            break;
                        }
                    }
                }
            }
            
            return isValid;
        }
        private TokenValidationParameters GetTokenValidationParameters()
        {
            var tokenKey = Encoding.ASCII.GetBytes(this._jwtSetting.SecurityKey);
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
