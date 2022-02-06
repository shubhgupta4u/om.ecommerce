using System.Threading.Tasks;

namespace om.shared.caching.Interfaces
{
    public interface IBaseCacheRepository<T>
    {
        Task<T> GetAsync(string key);
        Task WriteAsync(T user);
        Task WriteAsync(T user, int cacheExpiryTime);
        Task RemoveAsync(string key);
    }
}
