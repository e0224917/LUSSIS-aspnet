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
        public IEnumerable<Category> GetAllCategories()
        {
            return LUSSISContext.Categories.ToList();
        }

        public String GetCategoryInitial(string categoryId)
        {
            int catId = Int32.Parse(categoryId);   
            Category cat = LUSSISContext.Categories.Where(x => x.CategoryId == catId).First();
            return cat.CategoryName.Substring(0, 1);
        }

        public int GetLastRunningPlusOne(string initial)
        {
            List<Stationery> st = LUSSISContext.Stationeries.Where(x => x.ItemNum.StartsWith(initial)).ToList();
            List<int> haha = new List<int>();
            foreach (Stationery station in st)
            {
                haha.Add(Int32.Parse(station.ItemNum.Substring(1)));
            }
            haha.Sort();
            return (haha.Last() + 1);
        }

        public void AddSS(StationerySupplier stationerySupplier)
        {
            LUSSISContext.Set<StationerySupplier>().Add(stationerySupplier);
            LUSSISContext.SaveChanges();
        }

        public StationerySupplier GetSSByIdRank(string id, int rank)
        {
            return LUSSISContext.StationerySuppliers.Where(x => x.ItemNum == id && x.Rank == rank).FirstOrDefault();
        }

        
        public void DeleteStationerySUpplier(string itemNum)
        {
            List < StationerySupplier > ss = LUSSISContext.StationerySuppliers.Where(x => x.ItemNum == itemNum).ToList();
            foreach (StationerySupplier stationsupllier in ss)
            {
                LUSSISContext.StationerySuppliers.Remove(stationsupllier);
            }
                LUSSISContext.SaveChanges();
            
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

        public IEnumerable<StationerySupplier> GetAllStationerySuppliers()
        {
            return LUSSISContext.StationerySuppliers;
        }

        public IEnumerable<String> GetAllItemNum()
        {
            return LUSSISContext.Stationeries.Select(x => x.ItemNum).ToList();
        }

        public IEnumerable<Stationery> GetStationeryBySupplierId(int? id)
        {
            var q = from t1 in LUSSISContext.Stationeries
                    join t2 in LUSSISContext.StationerySuppliers
                    on t1.ItemNum equals t2.ItemNum
                    where t2.Supplier.SupplierId == id
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

        private class PendingPOQuantityByItem
        {
            public string ItemNum { get; set; }
            public int? Qty { get; set; }
        }
        public Dictionary<Supplier, List<Stationery>> GetOutstandingStationeryByAllSupplier()
        {
            //get stationery which has current qty<reorder level
            Dictionary<Supplier, List<Stationery>> dic = new Dictionary<Supplier, List<Stationery>>();
            //get list of pending PO stationery and qty
            List<Stationery> slist = LUSSISContext.Stationeries.Where(x => x.CurrentQty < x.ReorderLevel).ToList();
            var p = from t1 in LUSSISContext.PurchaseOrderDetails
                    join t2 in LUSSISContext.PurchaseOrders
                    on t1.PoNum equals t2.PoNum
                    where t2.Status == "pending" ||
                    t2.Status == "ordered" ||
                    t2.Status == "approved"
                    group t1 by t1.ItemNum into t3
                    select new PendingPOQuantityByItem
                    {
                        ItemNum = t3.FirstOrDefault().ItemNum,
                        Qty = t3.Sum(x => x.OrderQty) - t3.Sum(x => x.ReceiveQty)
                    };

            //get dictionary of supplier and qty 
            if (slist != null)
            {
                for (int i = 0; i < slist.ToList().Count; i++)
                {
                    string itemNum = slist[i].ItemNum;
                    int pendingPoQty = 0;
                    var q = p.Where(x => x.ItemNum == itemNum).ToList();
                    if (q.Count>0)
                        pendingPoQty = Convert.ToInt32(q.First().Qty);
                    if (slist[i].CurrentQty + pendingPoQty < slist[i].ReorderLevel)
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
            }
            return dic;
        }
        public List<String> GetItembyCategory(int c)
        {  
           
            List<String> stList = new List<String>();
            List<Stationery>l = LUSSISContext.Stationeries.Where(x => x.CategoryId == c).ToList();
            foreach(Stationery a in l)
            {
                stList.Add(a.ItemNum);
            }
            return stList;
        }





    }
}
