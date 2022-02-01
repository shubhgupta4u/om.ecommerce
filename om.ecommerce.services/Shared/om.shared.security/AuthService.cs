using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
    public class AuthService : IAuthService
    {
        private readonly JwtSetting _jwtSetting;
        private readonly OktaSetting _oktaSetting;
        private readonly IUserTokenRepository _userTokenRepository;
        public AuthService(IOptions<JwtSetting> options, IOptions<OktaSetting> oktaSetting, IUserTokenRepository userTokenRepository)
        {
            this._userTokenRepository = userTokenRepository;
            this._jwtSetting = options.Value;
            this._oktaSetting = oktaSetting.Value;
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
        public bool ValidateOktaToken(string jwt, out string email)
        {
            bool isValid = false;
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                                                // .well-known/oauth-authorization-server or .well-known/openid-configuration
                                                this._oktaSetting.OpenIdMetadataAddress,
                                                new OpenIdConnectConfigurationRetriever(),
                                                new HttpDocumentRetriever());

            var discoveryDocument = configurationManager.GetConfigurationAsync().Result;
            var signingKeys = discoveryDocument.SigningKeys;

            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = signingKeys,
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = true,
                ValidAudience = "api://default",//"0oa3rqwjeikxFYVOY5d7",
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = string.Format("https://{0}/oauth2/default", this._oktaSetting.OktaDomain)
            };
            IdentityModelEventSource.ShowPII = true;
            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

                var token= (JwtSecurityToken)rawValidatedToken;
                email = token.Claims.FirstOrDefault(x => x.Type == "sub").Value;
                isValid = true;
            }
            catch (SecurityTokenValidationException ex)
            {
                email = string.Empty;
            }
            catch (ArgumentException ex)
            {
                email = string.Empty;
            }
            return isValid;
        }

        public bool ValidateToken(string authToken, out IEnumerable<Claim> claims)
        {
            bool isValid = false;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(authToken, this.GetTokenValidationParameters(), out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                claims = jwtToken.Claims;
                if (jwtToken != null && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256) && claims.Any())
                {
                    UserTokenInfo userTokenInfo = this._userTokenRepository.GetAsync(claims.FirstOrDefault(x => x.Type == "nameid").Value).Result;
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
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException ex)
            {
                claims = null;
            }
            catch
            {
                claims = null;
            }
            return isValid;
        }
        public bool ValidateToken(string authToken, string allowedRoles, out int errorCode)
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
