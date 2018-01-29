using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Repositories
{
    public class StationeryRepository : Repository<Stationery, string>, IStationeryRepository
    {
        public int GetLastRunningPlusOne(string initial)
        {
            List<Stationery> st = LUSSISContext.Stationeries.Where(x => x.ItemNum.StartsWith(initial)).ToList();
            List<int> runningNum = new List<int>();
            foreach (Stationery station in st)
            {
                runningNum.Add(Int32.Parse(station.ItemNum.Substring(1)));
            }
            runningNum.Sort();
            return (runningNum.Last() + 1);
        }



 

        


       

        public IEnumerable<Stationery> GetByCategory(string category)
        {
            return LUSSISContext.Stationeries.Where(s => s.Category.CategoryName == category);
        }
        public IEnumerable<Stationery> GetByDescription(string Description)
        {
            return LUSSISContext.Stationeries.Where(s => s.Description.Contains(Description));
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
            return GetAll().Where(x => x.CategoryId == c).Select(x => x.ItemNum).ToList();
        }
    }
}
