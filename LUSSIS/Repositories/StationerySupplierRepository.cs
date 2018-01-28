using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
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

        public IEnumerable<StationerySupplier> GetAllStationerySuppliers()
        {
            return LUSSISContext.StationerySuppliers;
        }

    }
}