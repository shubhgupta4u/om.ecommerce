using MongoDB.Driver;
using om.account.repository.Interfaces;
using om.account.model;
using om.shared.dataaccesslayer;
using System;
using System.Threading.Tasks;

namespace om.account.repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IMongoBookDBContext context) : base(context)
        {
            
        }
        public async Task<User> GetByEmail(string email)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq("Email", email);
            return await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
        }
        public async Task<User> GetByMobile(string mobile)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq("Mobile", mobile);
            return await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
        }
        public async Task SetEmailVerified(string id)
        {
            User user = await this.Get(id);
            user.IsEmailVerfied = true;
            await this.Update(user);
        }
        public async Task SetMobileVerified(string id)
        {
            User user = await this.Get(id);
            user.IsMobileVerfied = true;
            await this.Update(user);
        }
    }

}
