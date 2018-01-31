using LUSSIS.Models;
using LUSSIS.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Constants;

namespace LUSSIS.Repositories
{
    //Authors: Koh Meng Guan
    public class StockAdjustmentRepository : Repository<AdjVoucher, int>, IStockAdjustmentRepository
    {
        public List<AdjVoucher> GetPendingAdjustmentList()
        {
            return GetAll().Where(x => x.Status == AdjustmentVoucherStatus.Pending).ToList();
        }

        public int GetPendingAdjustmentCount()
        {
            return GetAll().Where(x => x.Status == AdjustmentVoucherStatus.Pending).ToList().Count;
        }

        public List<AdjVoucher> GetPendingAdjustmentByRole(string role)
        {
            var resultList = new List<AdjVoucher>();
            var list = GetPendingAdjustmentList();
            if (role.Equals(Role.Supervisor))
            {
                foreach (var ad in list)
                {
                    var s = ad.Stationery.AverageCost;
                    var qty = ad.Quantity;
                    if (s * qty <= 250)
                    {
                        resultList.Add(ad);
                    }
                }
            }
            else if (role.Equals(Role.Manager))
            {
                foreach (var ad in list)
                {
                    var s = ad.Stationery.AverageCost;
                    var qty = ad.Quantity;
                    if (s * qty > 250)
                    {
                        resultList.Add(ad);
                    }
                }
            }

            return resultList;
        }

        public List<AdjVoucher> GetPendingAdjustmentByType(string type)
        {
            switch (type)
            {
                case "add":
                    return GetPendingAdjustmentList().Where(a => a.Quantity > 0).ToList();
                case "subtract":
                    return GetPendingAdjustmentList().Where(a => a.Quantity < 0).ToList();
                default:
                    return GetPendingAdjustmentList();
            }
        }
        
        public IEnumerable<AdjVoucher> FindAdjVoucherByText(string term)
        {
            term = term.ToLower();
            IEnumerable<AdjVoucher> adjList = LUSSISContext.AdjVouchers.Where(r =>
                r.RequestEmployee.FirstName.ToLower().Contains(term) ||
                r.RequestEmployee.LastName.ToLower().Contains(term) || r.Status.ToLower().Contains(term) ||
                r.Stationery.Description.ToLower().Contains(term) || r.Quantity.ToString().Contains(term) ||
                r.Reason.ToLower().Contains(term) || r.Remark.Contains(term));
            return adjList;
        }

        public IEnumerable<AdjVoucher> GetApprovedAdjVoucherByItem(string itemNum)
        {
            return LUSSISContext.AdjVouchers.Where(x => x.ItemNum == itemNum && x.Status == AdjustmentVoucherStatus.Approved);
        }

       public Stationery AddStockAdjustment(AdjVoucher adjVoucher)
        {
            LUSSISContext.Set<AdjVoucher>().Add(adjVoucher);
            LUSSISContext.SaveChanges();
            return LUSSISContext.Set<Stationery>().Find(adjVoucher.ItemNum);
        }
    }
}