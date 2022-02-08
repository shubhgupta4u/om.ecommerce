using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using om.shared.caching.Interfaces;
using om.shared.caching.Models;
using om.shared.security.Interfaces;
using om.shared.security.models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace om.shared.security
{
    public class AuthService : IAuthService
    {
        private readonly JwtSetting _jwtSetting;
        private readonly OktaSetting _oktaSetting;
        private readonly AzureAdSetting _azureAdSetting;
        private readonly IUserTokenRepository _userTokenRepository;
        public AuthService(IOptions<JwtSetting> options, IOptions<OktaSetting> oktaSetting, IOptions<AzureAdSetting> azureAdSetting, IUserTokenRepository userTokenRepository)
        {
            this._userTokenRepository = userTokenRepository;
            this._jwtSetting = options.Value;
            if(oktaSetting != null && oktaSetting.Value != null)
            {
                this._oktaSetting = oktaSetting.Value;
            }
            if (azureAdSetting != null && azureAdSetting.Value != null)
            {
                this._azureAdSetting = azureAdSetting.Value;
            }
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
            if (this._oktaSetting == null)
            {
                email = string.Empty;
                return false;
            }
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
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
                ValidateLifetime = false,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = true,
                ValidAudience = "api://default",//"0oa3rqwjeikxFYVOY5d7",
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = string.Format("https://{0}/oauth2/default", this._oktaSetting.OktaDomain)
            };
            var tokenHandler = new JwtSecurityTokenHandler();   
            //IdentityModelEventSource.ShowPII = true;
            try
            {
                // Throws an Exception as the token is invalid (expired, invalid-formatted, etc.)  
                var claimsPrincipal = new JwtSecurityTokenHandler()
                     .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

                var token = (JwtSecurityToken)rawValidatedToken;
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
        public bool ValidateMsalToken(string jwt, out string email)
        {
            bool isValid = false;
            if (this._azureAdSetting == null)
            {
                email = string.Empty;
                return false;
            }
            string myTenant = this._azureAdSetting.TenantId;
            var myAudience = this._azureAdSetting.Audience;
            var myIssuer = this._azureAdSetting.Issuer;
            var mySecret = this._azureAdSetting.SecretKey;
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            var stsDiscoveryEndpoint = String.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/.well-known/openid-configuration", myTenant);
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint,
                                                    new OpenIdConnectConfigurationRetriever(),
                                                    new HttpDocumentRetriever());

            var discoveryDocument = configurationManager.GetConfigurationAsync().Result;
            var signingKeys = discoveryDocument.SigningKeys;
            var validationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                IssuerSigningKeys = signingKeys,
                IssuerSigningKey = mySecurityKey,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = false,
                ValidateAudience = true,
                ValidAudience = myAudience,
                ValidateIssuer = true,
                ValidIssuer = myIssuer
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var validatedToken = (SecurityToken)new JwtSecurityToken();
            //IdentityModelEventSource.ShowPII = true;
            try
            {
                // Throws an Exception as the token is invalid (expired, invalid-formatted, etc.)  
                var claimsPrincipal = tokenHandler.ValidateToken(jwt, validationParameters, out validatedToken);
                email = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
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
            catch (Exception ex)
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
        public TokenValidationParameters GetTokenValidationParameters()
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

        public async Task<string> GenerateRefreshTokenAsync(string userId, string token, string remoteIpAddress)
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                string refreshToken = Convert.ToBase64String(randomNumber);

                UserTokenInfo userToken = await this._userTokenRepository.GetAsync(userId);
                if (userToken != null)
                {
                    userToken.PreviousToken = userToken.CurrentToken;
                    userToken.IsActive = true;
                    userToken.CurrentToken = token;
                    userToken.RefreshToken = refreshToken;
                    userToken.ModifiedDate = DateTimeOffset.UtcNow;
                }
                else
                {
                    userToken = new UserTokenInfo
                    {
                        IsActive = true,
                        TokenId = new Random().Next().ToString(),
                        UserId = userId,
                        RefreshToken = refreshToken,
                        RemoteIpAddress = remoteIpAddress,
                        ModifiedDate = DateTimeOffset.UtcNow,
                        CurrentToken = token
                    };
                }
                await this._userTokenRepository.WriteAsync(userToken, this._jwtSetting.ExpireTime);

                return refreshToken;
            }
        }

        public async Task<string> GenerateApiServiceTokenAsync(string idenetifier, DateTime tokenExpiry)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(this._jwtSetting.SecurityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity
                    (
                        new Claim[]
                        {
                            new Claim(ClaimTypes.Name,idenetifier),
                            new Claim(ClaimTypes.NameIdentifier,idenetifier),
                            new Claim(ClaimTypes.Email,idenetifier)
                        }
                    ),
                NotBefore = DateTime.Now,
                Expires = tokenExpiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var finalToken = tokenHandler.WriteToken(token);
            await this.GenerateRefreshTokenAsync(idenetifier, finalToken, string.Empty);
            return finalToken;
        }
    }
}
