using MongoDB.Driver;
using om.account.model;
using om.account.repository.Interfaces;
using om.shared.dataaccesslayer;
using System.Threading.Tasks;

namespace om.account.repository
{
    public class UserCredentialRepository:BaseRepository<UserCredential>,IUserCredentialRepository
    {
        public UserCredentialRepository(IMongoBookDBContext context) : base(context)
        {

        }
        public async Task<UserCredential> GetByUserId(string userId)
        {
            FilterDefinition<UserCredential> filter = Builders<UserCredential>.Filter.Eq("UserId", userId);
            return await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
        }
    }
}
