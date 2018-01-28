using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    public class StockAdjustmentRepository : Repository<AdjVoucher, int>, IStockAdjustmentRepository
    {
        public List<AdjVoucher> GetPendingAdjustmentList()
        {
            return GetAll().Where(x => x.Status == "pending").ToList();
        }

        public List<AdjVoucher> ViewPendingStockAdj(string role)
        {
            var resultList = new List<AdjVoucher>();
            List<AdjVoucher> list = GetPendingAdjustmentList();
            List<double> priceList = new List<double>();
            if (role.Equals("supervisor"))
            {
                foreach (AdjVoucher ad in list)
                {
                    double s = ((double) (ad.Stationery.AverageCost));
                    int qty = (int) ad.Quantity;
                    if ((s * qty) <= 250)
                    {
                        resultList.Add(ad);
                    }
                }
            }
            else if (role.Equals("manager"))
            {
                foreach (AdjVoucher ad in list)
                {
                    double s = ((double) (ad.Stationery.AverageCost));
                    int qty = (int) ad.Quantity;
                    if ((s * qty) > 250)
                    {
                        resultList.Add(ad);
                    }
                }
            }

            return resultList;
        }

        public int GetPendingStockAddQty()
        {
            List<AdjVoucher> list = GetPendingAdjustmentList();
            int total = 0;
            foreach (AdjVoucher adj in list)
            {
                int Quantity = (int) LUSSISContext.AdjVouchers.Select(x => x.Quantity).ToList()[0];
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
                int Quantity = (int) LUSSISContext.AdjVouchers.Select(x => x.Quantity).ToList()[0];
                if (Quantity < 0)
                {
                    total += Quantity;
                }
            }

            return total;
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

        public List<AdjVoucher> GetAllAdjVoucherSearch(string term)
        {
            term = term.ToLower();
            List<AdjVoucher> adjustments = LUSSISContext.AdjVouchers.Where(r =>
                r.RequestEmployee.FirstName.ToLower().Contains(term) ||
                r.RequestEmployee.LastName.ToLower().Contains(term) || r.Status.ToLower().Contains(term) ||
                r.Stationery.Description.ToLower().Contains(term) || r.Quantity.ToString().Contains(term) ||
                r.Reason.ToLower().Contains(term) || r.Remark.Contains(term)).ToList();
            return adjustments;
        }

        public IEnumerable<AdjVoucher> GetApprovedAdjVoucherByItem(string id)
        {
            return LUSSISContext.AdjVouchers.Where(x => x.ItemNum == id && x.Status == "approved");
        }
    }
}