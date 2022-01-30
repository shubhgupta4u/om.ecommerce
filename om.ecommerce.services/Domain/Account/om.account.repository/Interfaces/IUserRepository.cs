using om.account.model;
using om.shared.dataaccesslayer;
using System.Threading.Tasks;

namespace om.account.repository.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByEmail(string email);
        Task<User> GetByMobile(string mobile);
        Task SetEmailVerified(string id);
        Task SetMobileVerified(string id);
    }
}
