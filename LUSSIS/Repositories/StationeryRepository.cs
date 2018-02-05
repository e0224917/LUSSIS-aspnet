using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LUSSIS.Repositories
{
    //Authors: Koh Meng Guan
    public class StationeryRepository : Repository<Stationery, string>, IStationeryRepository
    {
        /// <summary>
        /// Method to generate the next item number for stationery
        /// </summary>
        /// <param name="initial"></param>
        /// <returns></returns>
        public int GetLastRunningPlusOne(string initial)
        {
            var stationerys = LUSSISContext.Stationeries.Where(x => x.ItemNum.StartsWith(initial)).ToList();
            var runningNum = new List<int>();
            foreach (var stationery in stationerys)
            {
                runningNum.Add(int.Parse(stationery.ItemNum.Substring(1)));
            }

            runningNum.Sort();
            return runningNum.Last() + 1;
        }

        public IEnumerable<Stationery> GetByCategory(string category)
        {
            return LUSSISContext.Stationeries.Where(s => s.Category.CategoryName == category);
        }

        public IEnumerable<Stationery> GetByDescription(string description)
        {
            return LUSSISContext.Stationeries.Where(s => s.Description.Contains(description));
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
            return q.AsEnumerable();
        }

        public Dictionary<Supplier, List<Stationery>> GetOutstandingStationeryByAllSupplier()
        {
            var dictionary = new Dictionary<Supplier, List<Stationery>>();

            //get list of pending PO stationery and qty
            var stationerys = GetAll().Where(x => x.AvailableQty < x.ReorderLevel).ToList();

            //fill dictionary
            foreach (var stationery in stationerys)
            {
                var primarySupplier = stationery.PrimarySupplier();
                if (dictionary.ContainsKey(primarySupplier))
                {
                    dictionary[primarySupplier].Add(stationery);
                }
                else
                {
                    dictionary.Add(primarySupplier, new List<Stationery> {stationery});
                }
            }

            return dictionary;
        }
    }
}