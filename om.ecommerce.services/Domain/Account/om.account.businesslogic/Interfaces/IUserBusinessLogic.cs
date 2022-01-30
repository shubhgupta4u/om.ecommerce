using om.account.model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace om.account.businesslogic.Interfaces
{
    public interface IUserBusinessLogic
    {
        Task<User> Create(CreateUserRequest obj);
        Task<User> Update(string userId, UpdateUserRequest obj);
        Task Delete(string id);
        Task<User> Get(string id);
        Task<IEnumerable<User>> Get();
        Task<User> GetByEmail(string email);
        Task<User> GetByMobile(string mobile);
        Task SetEmailVerified(string id);
        Task SetMobileVerified(string id);
        Task<ValidateCredentialResponse> ValidateCredential(ValidateCredentialRequest request);
    }
}
