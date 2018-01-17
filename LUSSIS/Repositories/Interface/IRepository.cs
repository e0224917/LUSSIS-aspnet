using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUSSIS.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="ID"></typeparam>
    interface IRepository<TEntity, ID> where TEntity : class
    {
        void Add(TEntity entity);

        Task<int> AddAsync(TEntity entity);

        IEnumerable<TEntity> GetAll();

        Task<IEnumerable<TEntity>> GetAllAsync();

        TEntity GetById(ID id);

        Task<TEntity> GetByIdAsync(ID id);

        void Delete(TEntity entity);

        Task<int> DeleteAsync(TEntity entity);

        void Update(TEntity entity);

        Task<int> UpdateAsync(TEntity entity);

        void Dispose();
    }
}
