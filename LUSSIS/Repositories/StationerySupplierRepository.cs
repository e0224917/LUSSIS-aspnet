using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    //Authors: Koh Meng Guan
    public class StationerySupplierRepository : Repository<StationerySupplier, int>, IStationerySupplierRepository
    {
        public StationerySupplier GetSSByIdRank(string id, int rank)
        {
            return LUSSISContext.StationerySuppliers.FirstOrDefault(x => x.ItemNum == id && x.Rank == rank);
        }

        public void DeleteStationerySupplier(string itemNum)
        {
            List<StationerySupplier> ss = LUSSISContext.StationerySuppliers.Where(x => x.ItemNum == itemNum).ToList();
            foreach (StationerySupplier stationsupllier in ss)
            {
                LUSSISContext.StationerySuppliers.Remove(stationsupllier);
            }
            LUSSISContext.SaveChanges();

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

        public void UpdateAll(IEnumerable<StationerySupplier> newList)
        {
            IEnumerable<StationerySupplier> oldList = GetAll();
            LUSSISContext.StationerySuppliers.RemoveRange(oldList);
            LUSSISContext.StationerySuppliers.AddRange(newList);
            LUSSISContext.SaveChanges();
        }

    }
}