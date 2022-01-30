using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using om.shared.security.Interfaces;
using om.shared.security.models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace om.shared.security
{
    public class UserTokenRepository: IUserTokenRepository
    {
        private readonly IDistributedCache _distributedCache;
        private readonly JwtSetting _jwtSetting;
        private const string TOKEN_CACHE_KEY_PREFIX = "token";
        public UserTokenRepository(IOptions<JwtSetting> options, IDistributedCache _distributedCache) {
            this._distributedCache = _distributedCache;
            this._jwtSetting = options.Value;
        }

        public async Task<UserTokenInfo> GetAsync(string userId)
        {
            var redisUserToken = await _distributedCache.GetAsync($"{TOKEN_CACHE_KEY_PREFIX}_{userId}");
            UserTokenInfo userTokenInfo = null;
            if (redisUserToken != null)
            {
                string serializedredisUserToken = Encoding.UTF8.GetString(redisUserToken);
                userTokenInfo = JsonConvert.DeserializeObject<UserTokenInfo>(serializedredisUserToken);
            }
            return userTokenInfo;
        }
        public async Task WriteAsync(UserTokenInfo user)
        {
            string serializedUserToken = JsonConvert.SerializeObject(user);
            var redisUserToken = Encoding.UTF8.GetBytes(serializedUserToken);
            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(this._jwtSetting.ExpireTime)).SetSlidingExpiration(TimeSpan.FromMinutes(this._jwtSetting.ExpireTime));
            await _distributedCache.SetAsync($"{TOKEN_CACHE_KEY_PREFIX}_{user.UserId}", redisUserToken, options);
        }
        public async Task RemoveAsync(string userid)
        {
            await _distributedCache.RemoveAsync($"{TOKEN_CACHE_KEY_PREFIX}_{userid}");
        }
    }
}
