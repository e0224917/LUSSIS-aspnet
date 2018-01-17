using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    public class StationeryRepository : Repository<Stationery, string>, IStationeryRepository
    {
        public StationeryRepository(LUSSISContext context) : base(context)
        {
        }

        public IEnumerable<Stationery> GetByCategory(string category)
        {
            return LUSSISContext.Stationeries.Where(s => s.Category.CategoryName == category);
        }

        public LUSSISContext LUSSISContext
        {
            get { return Context as LUSSISContext; }
        }
    }
}