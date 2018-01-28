using System;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories.Interface;

namespace LUSSIS.Repositories
{

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
            double result = 0;
            var list = GetPOByStatus("fulfilled").ToList();

            foreach (var po in list)
            {
                var pdList = po.PurchaseOrderDetails.ToList();

                foreach (var pod in pdList)
                {
                    result += GetPOAmountByPoNum(pod.PoNum);
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

        public double GetPOAmountByCategory(int categoryId, List<String>itemList)
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
        }

        public IEnumerable<ReceiveTransDetail> GetReceiveTransDetailByItem(string id)
        {
            return LUSSISContext.ReceiveTransDetails.Where(x => x.ItemNum == id);
        }

        public IEnumerable<PurchaseOrderDetail> GetPurchaseOrderDetailsByStatus(string status)
        {
            return LUSSISContext.PurchaseOrderDetails.Where(x => x.PurchaseOrder.Status == status);
        }

        public List<double> GetAmountByCategoryList(List<string> categoryIds, string supId, string from, string to)
        {
            var fromDate = Convert.ToDateTime(from).Date;
            var toDate = Convert.ToDateTime(to).Date;

            var result = new List<double>();

            foreach (var id in categoryIds)
            {
                var categotyId = int.Parse(id);
                var itemList = LUSSISContext.Stationeries
                    .Where(x=>x.CategoryId == categotyId).Select(x=>x.ItemNum).ToList();
                var supplierId = Convert.ToInt32(supId);
                double total = 0;
                foreach (var item in itemList)
                {
                    var s = from t1 in LUSSISContext.Disbursements
                            join t2 in LUSSISContext.DisbursementDetails
                            on t1.DisbursementId equals t2.DisbursementId
                            join t3 in LUSSISContext.StationerySuppliers
                            on t2.ItemNum equals t3.ItemNum
                            where t2.Stationery.ItemNum == item &&
                            t3.SupplierId == supplierId &&
                            (t1.CollectionDate >= fromDate && t1.CollectionDate <= toDate)
                            select new
                            {
                                price = (int)t2.Stationery.AverageCost,
                                Qty = (double)t2.ActualQty
                            };
                    foreach (var a in s)
                    {
                        total += a.Qty;
                    }
                }

                result.Add(total);

            }

            return result;
        }

        public List<double> GetAmountBySupplierList(List<String> supplierIds, String category, String from, String to)
        {
            DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            double total = 0;

            var resultList = new List<double>();
            var catId = Convert.ToInt32(category);
            foreach (var sup in supplierIds)
            {
                var supId = Convert.ToInt32(sup);

                total = 0;
                var q = from t1 in LUSSISContext.Disbursements
                        join t2 in LUSSISContext.DisbursementDetails
                        on t1.DisbursementId equals t2.DisbursementId
                        join t3 in LUSSISContext.StationerySuppliers
                        on t2.ItemNum equals t3.ItemNum
                        join t4 in LUSSISContext.Stationeries
                        on t2.ItemNum equals t4.ItemNum
                        where t3.SupplierId == supId
                        && t4.CategoryId == catId
                        && (t1.CollectionDate >= fromDate && t1.CollectionDate <= toDate)
                        select new
                        {
                            price = (int)t2.Stationery.AverageCost,
                            Qty = (double)t2.ActualQty
                        };
                foreach (var a in q)
                {
                    total += a.price * a.Qty;
                }
                resultList.Add(total);
            }

            return resultList;
        }
    }
}
