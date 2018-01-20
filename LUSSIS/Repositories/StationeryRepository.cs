using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    public class StationeryRepository : Repository<Stationery, string>, IStationeryRepository
    {
        public StationeryRepository()
        {
        }

        public IEnumerable<Stationery> GetByCategory(string category)
        {
            return LUSSISContext.Stationeries.Where(s => s.Category.CategoryName == category);
        }
        public IEnumerable<Stationery> GetByDescription(string Description)
        {
            return LUSSISContext.Stationeries.Where(s => s.Description.Contains(Description));
        }
        public IEnumerable<String> GetAllCategory()
        {
            List<String> slist = new List<string>();
            List<Category> clist = LUSSISContext.Categories.ToList();
            foreach (Category c in clist)
            {
                slist.Add(c.CategoryName);
            }
            return slist;
        }

        public List<String> GetAllItemNum()
        {
            List<String> slist = new List<string>();
            List<Stationery> stlist = LUSSISContext.Stationeries.ToList();
            foreach (Stationery st in stlist)
            {
                slist.Add(st.ItemNum);
            }
            return slist;
        }
    }
}