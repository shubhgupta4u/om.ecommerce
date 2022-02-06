using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using om.shared.caching.Interfaces;
using om.shared.caching.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace om.shared.caching.Repository
{
    public class UserTokenRepository: IUserTokenRepository
    {
        private readonly IDistributedCache _distributedCache;
        private const string TOKEN_CACHE_KEY_PREFIX = "token";
        public UserTokenRepository(IDistributedCache _distributedCache) {
            this._distributedCache = _distributedCache;
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
        public async Task WriteAsync(UserTokenInfo user,int cacheExpiryTime)
        {
            string serializedUserToken = JsonConvert.SerializeObject(user);
            var redisUserToken = Encoding.UTF8.GetBytes(serializedUserToken);
            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(cacheExpiryTime)).SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpiryTime));
            await _distributedCache.SetAsync($"{TOKEN_CACHE_KEY_PREFIX}_{user.UserId}", redisUserToken, options);
        }
        public async Task WriteAsync(UserTokenInfo user)
        {
            string serializedUserToken = JsonConvert.SerializeObject(user);
            var redisUserToken = Encoding.UTF8.GetBytes(serializedUserToken);
            await _distributedCache.SetAsync($"{TOKEN_CACHE_KEY_PREFIX}_{user.UserId}", redisUserToken);
        }
        public async Task RemoveAsync(string userid)
        {
            await _distributedCache.RemoveAsync($"{TOKEN_CACHE_KEY_PREFIX}_{userid}");
        }
    }
}
