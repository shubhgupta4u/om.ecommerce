using om.shared.security.models;
using System.Threading.Tasks;

namespace om.shared.security.Interfaces
{
    public interface IUserTokenRepository
    {
        Task<UserTokenInfo> GetAsync(string userId);
        Task WriteAsync(UserTokenInfo user);
        Task RemoveAsync(string userid);
    }
}
