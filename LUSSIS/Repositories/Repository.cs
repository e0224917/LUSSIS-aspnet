using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LUSSIS.Repositories
{
    public class Repository<TEntity, ID> : IRepository<TEntity, ID> where TEntity : class
    {
        protected readonly LUSSISContext LUSSISContext;

        public Repository()
        {
            LUSSISContext = new LUSSISContext();
        }

        public void Add(TEntity entity)
        {
            LUSSISContext.Set<TEntity>().Add(entity);
            LUSSISContext.SaveChanges();
        }

        public async Task<int> AddAsync(TEntity entity)
        {
            LUSSISContext.Set<TEntity>().Add(entity);
            return await LUSSISContext.SaveChangesAsync();
        }

        public void Delete(TEntity entity)
        {
            LUSSISContext.Set<TEntity>().Remove(entity);
            LUSSISContext.SaveChanges();
        }

        public async Task<int> DeleteAsync(TEntity entity)
        {
            LUSSISContext.Set<TEntity>().Remove(entity);
            return await LUSSISContext.SaveChangesAsync();
        }

        public IEnumerable<TEntity> GetAll()
        {
            return LUSSISContext.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await LUSSISContext.Set<TEntity>().ToListAsync();
        }

        public TEntity GetById(ID id)
        {
            return LUSSISContext.Set<TEntity>().Find(id);
        }

        public async Task<TEntity> GetByIdAsync(ID id)
        {
            return await LUSSISContext.Set<TEntity>().FindAsync(id);
        }

        public void Update(TEntity entity)
        {
            LUSSISContext.Entry(entity).State = EntityState.Modified;
            LUSSISContext.SaveChanges();
        }

        public async Task<int> UpdateAsync(TEntity entity)
        {
            LUSSISContext.Entry(entity).State = EntityState.Modified;
            return await LUSSISContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            LUSSISContext.Dispose();
        }
    }
}