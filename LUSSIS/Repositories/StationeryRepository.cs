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

        public IEnumerable<Stationery> GetStationeryBySupplierId(int? id)
        {
            var q = from t1 in LUSSISContext.Stationeries
                    join t2 in LUSSISContext.StationerySuppliers
                    on t1.ItemNum equals t2.ItemNum
                    where t2.Supplier.SupplierId==id
                    select t1;
            return q.AsEnumerable<Stationery>();
        }

        public IEnumerable<StationerySupplier> GetStationerySupplierBySupplierId(int? id)
        {
            var q = from t1 in LUSSISContext.Stationeries
                    join t2 in LUSSISContext.StationerySuppliers
                    on t1.ItemNum equals t2.ItemNum
                    where t2.Supplier.SupplierId == id
                    select t2;
            return q.AsEnumerable<StationerySupplier>();
        }

        public Dictionary<Supplier, List<Stationery>> GetOutstandingStationeryByAllSupplier()
        {
            Dictionary<Supplier, List<Stationery>> dic = new Dictionary<Supplier, List<Stationery>>();
            List<Stationery> slist= LUSSISContext.Stationeries.Where(x => x.CurrentQty < x.ReorderLevel).ToList();
            if (slist != null)
            {
                for (int i = 0; i < slist.Count; i++)
                {
                    Supplier primarySupplier = slist[i].PrimarySupplier();
                    if (dic.ContainsKey(primarySupplier))
                    {
                        List<Stationery> value = null;
                        dic.TryGetValue(primarySupplier, out value);
                        value.Add(slist[i]);
                    }
                    else
                    {
                        dic.Add(primarySupplier, new List<Stationery>() { slist[i] });
                    }
                }
            }
            return dic;
        }
    }
}