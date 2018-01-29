using System;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LUSSIS.Constants;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories.Interface;
using static LUSSIS.Constants.POStatus;

namespace LUSSIS.Repositories
{
    //Authors: Douglas Lee Kiat Hui, May Zin Ko
    public class PORepository : Repository<PurchaseOrder, int>, IPORepository
    {
        public List<PurchaseOrder> GetPendingApprovalPO()
        {
            IEnumerable<PurchaseOrder> list = GetAll().Where(x => x.Status == "pending");
            return list.ToList();
        }

        public List<PurchaseOrder> GetApprovedPO()
        {
            var list = GetAll().Where(x => x.Status == "approved");
            return list.ToList();
        }

        public List<PurchaseOrder> GetPOByStatus(string status)
        {
            var list = GetAll().Where(x => x.Status == status);
            return list.ToList();
        }

        public int GetPendingPOCount()
        {
            List<PurchaseOrder> list = GetPendingApprovalPO();
            return list.Capacity;
        }
        /////

        public List<PendingPurchaseOrderDTO> GetPendingApprovalPODTO()
        {
            var list = GetPendingApprovalPO();
            var poDtoList = new List<PendingPurchaseOrderDTO>();
            foreach (var po in list)
            {
                var poDto = new PendingPurchaseOrderDTO
                {
                    PoNum = po.PoNum,
                    OrderEmp = po.OrderEmployee.FirstName,
                    Supplier = po.Supplier.SupplierName,
                    CreateDate = po.CreateDate,
                    TotalCost = GetPOAmountByPoNum(po.PoNum)
                };
                poDtoList.Add(poDto);
            }
            return poDtoList;
        }



        public double GetPOAmountByPoNum(int poNum)
        {
            var pdList = LUSSISContext.PurchaseOrderDetails.Where(x => x.PoNum == poNum).ToList();
            double total = 0;
            foreach (var pod in pdList)
            {
                var qty = pod.OrderQty;
                var unitPrice = pod.UnitPrice;
                total += qty * unitPrice;
            }

            return total;
        }

        public double GetPendingPOTotalAmount()
        {
            double result = 0;
            var list = GetPendingApprovalPO();

            foreach (var po in list)
            {
                result += GetPOAmountByPoNum(po.PoNum);
            }
            return result;
        }


        public double GetPOTotalAmount()
        {
            // DateTime toDate = DateTime.ParseExact(DateTime.Today, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.Parse("2018-01-31");
            DateTime fromDate = DateTime.Parse("2017-11-01");
            double result = 0;
            var q = from t1 in LUSSISContext.ReceiveTrans
                    join t2 in LUSSISContext.ReceiveTransDetails
                    on t1.ReceiveId equals t2.ReceiveId
                    join t3 in LUSSISContext.Stationeries
                    on t2.ItemNum equals t3.ItemNum
                    where t1.ReceiveId == t2.ReceiveId
                    && (t1.ReceiveDate >= fromDate && t1.ReceiveDate <= toDate)
                    select new
                    {
                        price = (int)t3.AverageCost,
                        qty = (double)t2.Quantity
                    };

            foreach (var a in q)
            {
                result += a.qty*a.price;
            }
            return result;
        }

        public void UpDatePOStatus(int i, String status)
        {
            var p = GetById(i);
            p.Status = status;
            p.ApprovalDate = DateTime.Today;
            Update(p);
        }

        /* public double GetPOAmountByCategory(int categoryId, List<String>itemList)
         {
             double total = 0;
             foreach (var e in itemList)
             {
                 var pdList = LUSSISContext.PurchaseOrderDetails.Where(x => x.ItemNum.Equals(e)).ToList();
                 foreach (var pod in pdList)
                 {
                     var qty = pod.OrderQty;
                     var unitPrice = pod.UnitPrice;
                     total += qty * unitPrice;

                 }
             }

             return total;
         }

         public List<double> GetPOByCategory()
         {
             var list = new List<double>();
             var categoryIds = LUSSISContext.Categories.Select(x => x.CategoryId).ToList();

             foreach (var id in categoryIds)
             {
                 var itemList = LUSSISContext.Stationeries.Where(x => x.CategoryId == id)
                     .Select(x => x.ItemNum).ToList();
                 list.Add(GetPOAmountByCategory(id, itemList));
             }

             return list;
         }*/
        public List<double> GetPOByCategory()
        {
            var list = new List<double>();
            DateTime toDate = DateTime.Parse("2018-01-31");
            DateTime fromDate = DateTime.Parse("2017-11-01");
            var categoryIds = LUSSISContext.Categories.Select(x => x.CategoryId).ToList();
            foreach (int catId in categoryIds)
            {
                double total = 0;
                var q = from t1 in LUSSISContext.ReceiveTrans
                        join t2 in LUSSISContext.ReceiveTransDetails
                        on t1.ReceiveId equals t2.ReceiveId
                        join t3 in LUSSISContext.Stationeries
                        on t2.ItemNum equals t3.ItemNum
                        where t3.CategoryId == catId 
                        && (t1.ReceiveDate <= toDate && t1.ReceiveDate >= fromDate)
                        select new
                        {
                            price = (int)t3.AverageCost,
                            Qty = (double)t2.Quantity
                        };

                foreach (var a in q)
                {
                    total += a.price * a.Qty;
                }
                list.Add(total);
            }
            return list;
        }

        public IEnumerable<ReceiveTransDetail> GetReceiveTransDetailByItem(string id)
        {
            return LUSSISContext.ReceiveTransDetails.Where(x => x.ItemNum == id);
        }

        public IEnumerable<PurchaseOrderDetail> GetPurchaseOrderDetailsByStatus(string status)
        {
            return LUSSISContext.PurchaseOrderDetails.Where(x => x.PurchaseOrder.Status == status);
        }

       
    }
}
