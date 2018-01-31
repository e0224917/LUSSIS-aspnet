using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Constants;

namespace LUSSIS.Repositories
{
    //Authors: Koh Meng Guan
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


        public Dictionary<Supplier, List<Stationery>> GetOutstandingStationeryByAllSupplier()
        {
            Dictionary<Supplier, List<Stationery>> dic = new Dictionary<Supplier, List<Stationery>>();


            //get qty of PO not approved yet
            var pendingQty = LUSSISContext.PurchaseOrderDetails
                .Where(x => x.PurchaseOrder.Status == "pending")
                .GroupBy(x=>x.ItemNum)
                .ToDictionary(x=>x.Key,x=> x.Sum(y => y.OrderQty));

            //get list of pending PO stationery and qty
            List<Stationery> slist = GetAll().Where(x => x.AvailableQty + (pendingQty.ContainsKey(x.ItemNum)?pendingQty[x.ItemNum]:0) < x.ReorderLevel).ToList();

            //fill dictionary
            if (slist != null)
            {
                foreach (Stationery s in slist)
                {
                    s.AvailableQty += pendingQty.ContainsKey(s.ItemNum) ? pendingQty[s.ItemNum] : 0;
                    Supplier primarySupplier = s.PrimarySupplier();
                    if (dic.ContainsKey(primarySupplier))
                    {
                        dic[primarySupplier].Add(s);
                    }
                    else
                    {
                        dic.Add(primarySupplier, new List<Stationery>() { s });
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
