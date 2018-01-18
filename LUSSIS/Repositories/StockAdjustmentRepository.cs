using LUSSIS.Models;
using LUSSIS.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    public class StockAdjustmentRepository : Repository<AdjVoucher, int>, IStockAdjustmentRepository
    {
        public StockAdjustmentRepository() { }

        public List<AdjVoucher> GetPendingAdjustmentList()
        {
            IEnumerable<AdjVoucher> list = LUSSISContext.AdjVouchers.Where(x => x.Status=="Pending").ToList<AdjVoucher>();
            return list.ToList();

            
        }

        


    }
}
