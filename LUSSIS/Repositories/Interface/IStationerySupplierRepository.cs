using System.Collections.Generic;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    public interface IStationerySupplierRepository
    {
        void DeleteStationerySupplier(string itemNum);
        StationerySupplier GetSSByIdRank(string id, int rank);
    }
}