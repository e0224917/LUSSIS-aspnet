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

        public int GetPendingStockAddQty()
        {
            List<AdjVoucher> list = GetPendingAdjustmentList();
            int total = 0;
            foreach (AdjVoucher adj in list)
            {
                int Quantity = (int)LUSSISContext.AdjVouchers.Select(x => x.Quantity).ToList()[0];
                if (Quantity > 0)
                {
                    total += Quantity;
                }
            }


            return total;
        }
        public int GetPendingStockSubtractQty()
        {
            List<AdjVoucher> list = GetPendingAdjustmentList();
            int total = 0;
            foreach (AdjVoucher adj in list)
            {
                int Quantity = (int)LUSSISContext.AdjVouchers.Select(x => x.Quantity).ToList()[0];
                if (Quantity < 0)
                {
                    total += Quantity;
                }
            }
            return total;
        }
        public int GetPendingStockCount()
        {
            List<AdjVoucher> list = GetPendingAdjustmentList();
            return list.Count();
        }


        public void UpDateAdjustmentStatus(int i, String status, String comment)
        {
            AdjVoucher a = new AdjVoucher();
            a = GetById(i);
            a.Status = status;
            a.Remark = comment;
            a.ApprovalDate = DateTime.Today;
            Update(a);
           
            

        }





    }
}
