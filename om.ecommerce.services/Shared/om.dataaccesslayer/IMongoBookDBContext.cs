using MongoDB.Driver;

namespace om.shared.dataaccesslayer
{
    public interface IMongoBookDBContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}
