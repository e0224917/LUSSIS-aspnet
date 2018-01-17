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
    interface IRepository<TEntity, ID> where TEntity: class
    {
        void Add(TEntity entity);

        IEnumerable<TEntity> GetAll();

        TEntity GetById(ID id);

        Task<TEntity> GetByIdAsync(ID id);

        void Delete(TEntity entity);

        void Update(TEntity entity);
    }
}
