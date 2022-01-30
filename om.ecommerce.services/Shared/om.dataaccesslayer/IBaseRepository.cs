using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace om.shared.dataaccesslayer
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task Create(TEntity obj);
        Task Update(TEntity obj);
        Task Delete(string id);
        Task<TEntity> Get(string id);
        Task<IEnumerable<TEntity>> Get();
    }
}
