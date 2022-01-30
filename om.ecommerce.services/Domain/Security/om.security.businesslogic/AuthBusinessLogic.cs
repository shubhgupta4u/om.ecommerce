using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using om.security.businesslogic.Interfaces;
using om.security.models;
using om.shared.security;
using om.shared.security.Interfaces;
using om.shared.security.models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace om.security.businesslogic
{
    public class AuthBusinessLogic: IAuthBusinessLogic,IDisposable
    {        
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        private readonly JwtSetting _jwtSetting;
        private readonly AccountApiEndPoints _accountApiEndPoints;
        private bool disposedValue;
        private IUserTokenRepository _userTokenRepository;
        private readonly IAuthService authService;

        public AuthBusinessLogic(IOptions<JwtSetting> jwtSetting, 
                                 IOptions<AccountApiEndPoints> accountApiEndPoints,
                                 IHttpClientFactory httpClientFactory,
                                 IUserTokenRepository userTokenRepository,
                                 IAuthService authService)
        {
            this._httpClientFactory = httpClientFactory;
            this._userTokenRepository = userTokenRepository;
            this.authService = authService;
            this._httpClient = _httpClientFactory.CreateClient();
            this._jwtSetting = jwtSetting.Value;
            this._accountApiEndPoints = accountApiEndPoints.Value;
            this._httpClient.BaseAddress = new Uri(this._accountApiEndPoints.BaseUri);
        }
        public async Task<ValidateCredentialResponse> AuthenticateAsync(string userName, string password)
        {
            ValidateCredentialRequest validateCredRequest = new ValidateCredentialRequest
            {
                Password = password,
                UserName = userName
            };
            ValidateCredentialResponse response = null;
            var validateCredResponseJson = new StringContent(
               JsonConvert.SerializeObject(validateCredRequest),
               Encoding.UTF8, Application.Json);
            var httpResponseMessage = await this._httpClient.PostAsync(this._accountApiEndPoints.ValidateUserCredential, validateCredResponseJson);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentJson = await httpResponseMessage.Content.ReadAsStringAsync();
                response = JsonConvert.DeserializeObject<ValidateCredentialResponse>(contentJson);
            }
            return response;
        }
        public async Task<TokenResponse> GenerateTokenAsync(ValidateCredentialResponse validateCredResponse, string remoteIpAddress)
        {
            TokenResponse response = new TokenResponse();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(this._jwtSetting.SecurityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity
                    (
                        new Claim[]
                        {
                            new Claim(ClaimTypes.Name,validateCredResponse.User.UserName),
                            new Claim(ClaimTypes.NameIdentifier,validateCredResponse.User.UserId),
                            new Claim(ClaimTypes.Email,validateCredResponse.User.Email),
                            new Claim(ClaimTypes.MobilePhone,validateCredResponse.User.Mobile)
                        }
                    ),
                NotBefore = DateTime.Now,
                Expires =DateTime.Now.AddMinutes(this._jwtSetting.ExpireTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey),SecurityAlgorithms.HmacSha256)
            };
            if(validateCredResponse.User.UserRoles != null)
            {
                foreach(string role in validateCredResponse.User.UserRoles)
                {
                    tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }           
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var finalToken = tokenHandler.WriteToken(token);
            response.RefreshToken = await this.GenerateRefreshTokenAsync(validateCredResponse.User.UserId, finalToken, remoteIpAddress); 
            response.JwtToken = finalToken;
            return response;
        }
        public async Task<TokenResponse> GenerateRefreshTokenAsync(TokenResponse tokenResponse, string remoteIpAddress)
        {
            IEnumerable<Claim> claims;
            bool isValid = this.authService.ValidateToken(tokenResponse.JwtToken, out claims);
            if (isValid && claims.Any(x => x.Type == "nameid"))
            {
                var userId = claims.FirstOrDefault(x => x.Type == "nameid").Value;
                UserTokenInfo userToken = await this._userTokenRepository.GetAsync(userId);
                if (userToken.RefreshToken == tokenResponse.RefreshToken && userToken.RemoteIpAddress == remoteIpAddress)
                {
                    return await this.GenerateTokenAsync(userId, claims, remoteIpAddress);
                }
            }
            return null;
        }
        public async Task Logout(string userId)
        {
            await this._userTokenRepository.RemoveAsync(userId);
        }
        private async Task<TokenResponse> GenerateTokenAsync(string userId, IEnumerable<Claim> claims, string remoteIpAddress)
        {
            TokenResponse response = new TokenResponse();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(this._jwtSetting.SecurityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity
                    (
                        claims
                    ),
                Expires = DateTime.Now.AddMinutes(this._jwtSetting.ExpireTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var finalToken = tokenHandler.WriteToken(token);
            response.RefreshToken = await this.GenerateRefreshTokenAsync(userId, finalToken, remoteIpAddress);
            response.JwtToken = finalToken;
            return response;
        }
        private async Task<string> GenerateRefreshTokenAsync(string userId,string token, string remoteIpAddress)
        {
            var randomNumber = new byte[32];
            using(var randomNumberGenerator = RandomNumberGenerator.Create())
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
                                    RefreshToken = refreshToken , 
                                    RemoteIpAddress = remoteIpAddress,
                                    ModifiedDate = DateTimeOffset.UtcNow,
                                    CurrentToken = token 
                                };                    
                }
                await this._userTokenRepository.WriteAsync(userToken);

                return refreshToken;
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && this._httpClient != null)
                {
                    // TODO: dispose managed state (managed objects)
                    this._httpClient.Dispose();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._httpClient = null;
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~AuthBusinessLogic()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
