using om.account.model;
using om.shared.dataaccesslayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace om.account.repository.Interfaces
{
    public interface IUserCredentialRepository : IBaseRepository<UserCredential>
    {
        Task<UserCredential> GetByUserId(string userId);
    }
}
