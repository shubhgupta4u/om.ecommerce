using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using om.shared.caching.Interfaces;
using om.shared.caching.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace om.shared.caching.Repository
{
    public class SignalRConnectionRepository: ISignalRConnectionRepository
    {
        private readonly IDistributedCache _distributedCache;
        private const string SIGNALR_CACHE_KEY_PREFIX = "signalr";
        public SignalRConnectionRepository(IDistributedCache _distributedCache)
        {
            this._distributedCache = _distributedCache;
        }

        public async Task<SignalRConnection> GetAsync(string userId)
        {
            var redisSignalrCon = await _distributedCache.GetAsync($"{SIGNALR_CACHE_KEY_PREFIX}_{userId}");
            SignalRConnection userTokenInfo = null;
            if (redisSignalrCon != null)
            {
                string serializedredisSignalrCon = Encoding.UTF8.GetString(redisSignalrCon);
                userTokenInfo = JsonConvert.DeserializeObject<SignalRConnection>(serializedredisSignalrCon);
            }
            return userTokenInfo;
        }
        public async Task WriteAsync(SignalRConnection user, int cacheExpiryTime)
        {
            string serializedUserToken = JsonConvert.SerializeObject(user);
            var redisSignalrCon = Encoding.UTF8.GetBytes(serializedUserToken);
            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(cacheExpiryTime)).SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpiryTime));
            await _distributedCache.SetAsync($"{SIGNALR_CACHE_KEY_PREFIX}_{user.UserId}", redisSignalrCon, options);
        }
        public async Task WriteAsync(SignalRConnection user)
        {
            string serializedUserToken = JsonConvert.SerializeObject(user);
            var redisSignalrCon = Encoding.UTF8.GetBytes(serializedUserToken);
            await _distributedCache.SetAsync($"{SIGNALR_CACHE_KEY_PREFIX}_{user.UserId}", redisSignalrCon);
        }
        public async Task RemoveAsync(string userid)
        {
            await _distributedCache.RemoveAsync($"{SIGNALR_CACHE_KEY_PREFIX}_{userid}");
        }
    }
}
