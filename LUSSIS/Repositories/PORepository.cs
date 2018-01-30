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
            return GetAll().Where(x => x.Status == Pending).ToList();
        }

        public List<PurchaseOrder> GetApprovedPO()
        {
            var list = GetAll().Where(x => x.Status == Approved);
            return list.ToList();
        }

        public List<PurchaseOrder> GetPOByStatus(string status)
        {
            var list = GetAll().Where(x => x.Status.ToUpper() == status.ToUpper());
            return list.ToList();
        }

        public int GetPendingPOCount()
        {
            return GetPendingApprovalPO().Count;
        }
        
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


        public double GetPOTotalAmount(List<String>fromList)
        {
            double result = 0;
            foreach (String from in fromList)
            {
                DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var q = from t1 in LUSSISContext.ReceiveTrans
                        join t2 in LUSSISContext.ReceiveTransDetails
                        on t1.ReceiveId equals t2.ReceiveId
                        join t3 in LUSSISContext.Stationeries
                        on t2.ItemNum equals t3.ItemNum
                        where t1.ReceiveId == t2.ReceiveId
                        && (t1.ReceiveDate.Month==fromDate.Month && t1.ReceiveDate.Year==fromDate.Year)
                        select new
                        {
                            price = (int)t3.AverageCost,
                            qty = (double)t2.Quantity
                        };

                foreach (var a in q)
                {
                    result += a.qty * a.price;
                }
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

       
        public List<double> GetPOByCategory()
        {
            var list = new List<double>();
            List<String> fromList = new List<String>();
            fromList.Add(DateTime.Now.AddMonths(-3).ToString("dd/MM/yyyy"));
            fromList.Add(DateTime.Now.AddMonths(-2).ToString("dd/MM/yyyy"));
            fromList.Add(DateTime.Now.AddMonths(-1).ToString("dd/MM/yyyy"));
            var categoryIds = LUSSISContext.Categories.Select(x => x.CategoryId).ToList();

            foreach (int catId in categoryIds)
            {
                double total = 0;
                foreach (String from in fromList)
                {
                DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var q = from t1 in LUSSISContext.ReceiveTrans
                            join t2 in LUSSISContext.ReceiveTransDetails
                            on t1.ReceiveId equals t2.ReceiveId
                            join t3 in LUSSISContext.Stationeries
                            on t2.ItemNum equals t3.ItemNum
                            where t3.CategoryId == catId
                            && (t1.ReceiveDate.Month==fromDate.Month && t1.ReceiveDate.Year==fromDate.Year)
                            select new
                            {
                                price = (int)t3.AverageCost,
                                Qty = (double)t2.Quantity
                            };

                    foreach (var a in q)
                    {
                        total += a.price * a.Qty;
                    }
                   
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
            return LUSSISContext.PurchaseOrderDetails.Where(x => x.PurchaseOrder.Status.ToUpper() == status.ToUpper());
        }
        public IEnumerable<PurchaseOrderDetail> GetPurchaseOrderDetailsById(int poNo)
        {
            return LUSSISContext.PurchaseOrderDetails.Where(x => x.PurchaseOrder.PoNum == poNo);
        }


    }
}
