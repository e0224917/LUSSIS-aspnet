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
        public PORepository() { }
        StationeryRepository sr = new StationeryRepository();

        public List<PurchaseOrder> GetPendingApprovalPO()
        {
            IEnumerable<PurchaseOrder> list = GetAll().Where(x => x.Status == "pending");
            return list.ToList();
        }

        public List<PurchaseOrder> GetApprovedPO()
        {
            IEnumerable<PurchaseOrder> list = GetAll().Where(x => x.Status == "approved");
            return list.ToList();
        }

        public List<PurchaseOrder> GetPOByStatus(string status)
        {
            IEnumerable<PurchaseOrder> list = GetAll().Where(x => x.Status == status);
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
            PendingPurchaseOrderDTO poDTO;
            List<PurchaseOrder> list = GetPendingApprovalPO();
            List<PendingPurchaseOrderDTO> poDTOList = new List<PendingPurchaseOrderDTO>();
            foreach (PurchaseOrder po in list)
            {
                poDTO = new PendingPurchaseOrderDTO();
                poDTO.PoNum = po.PoNum;
                poDTO.OrderEmp = po.OrderEmployee.FirstName;
                poDTO.Supplier = po.Supplier.SupplierName;
                poDTO.CreateDate = po.CreateDate;
                poDTO.TotalCost = GetPOAmountByPoNum(po.PoNum);
                poDTOList.Add(poDTO);

            }
            return poDTOList;
        }



        public double GetPOAmountByPoNum(int poNum)
        {
            List<PurchaseOrderDetail> pd_list = LUSSISContext.PurchaseOrderDetails.Where(x => x.PoNum == poNum).ToList();
            double total = 0;
            foreach (PurchaseOrderDetail pod in pd_list)
            {


                int qty = (int)pod.OrderQty;
                double unit_price = (double)pod.UnitPrice;
                total += qty * unit_price;


            }
            return total;


        }
        public double GetPendingPOTotalAmount()
        {
            double result = 0;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            list = GetPendingApprovalPO();

            foreach (PurchaseOrder po in list)
            {

                result += GetPOAmountByPoNum(po.PoNum);
            }
            return result;
        }


        public double GetPOTotalAmount()
        {
            double result = 0;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            list = GetPOByStatus("fulfilled").ToList<PurchaseOrder>();

            foreach (PurchaseOrder po in list)
            {
                List<PurchaseOrderDetail> pd_list = po.PurchaseOrderDetails.ToList();

                foreach (PurchaseOrderDetail pod in pd_list)
                {
                    result += GetPOAmountByPoNum(pod.PoNum);
                }

            }
            return result;
        }


        public void UpDatePOStatus(int i, String status)
        {
            PurchaseOrder p = GetById(i);
            p.Status = status;
            p.ApprovalDate = DateTime.Today;
            Update(p);
        }

        public double GetPOAmountByCategory(int categoryId)
        {
            double total = 0;
            List<String> ItemList = new List<String>();
            ItemList = sr.GetItembyCategory(categoryId);
            List<PurchaseOrderDetail> pd_list = new List<PurchaseOrderDetail>();
            foreach (String e in ItemList)
            {
                pd_list = LUSSISContext.PurchaseOrderDetails.Where(x => x.ItemNum.Equals(e)).ToList<PurchaseOrderDetail>();
                foreach (PurchaseOrderDetail pod in pd_list)
                {
                    int qty = (int)pod.OrderQty;
                    double unit_price = (double)pod.UnitPrice;
                    total += qty * unit_price;


                }
            }

            return total;


        }
        public List<double> GetPOByCategory()
        {
            List<double> list = new List<double>();
            List<int> Cat = LUSSISContext.Categories.Select(x => x.CategoryId).ToList();

            foreach (int i in Cat)
            {
                list.Add(GetPOAmountByCategory(i));
            }
            return list;

        }

        public void ValidateReceiveTrans(ReceiveTran receive)
        {
            PurchaseOrder po = GetById(receive.PoNum);
            int? totalQty = 0;
            foreach (ReceiveTransDetail rdetail in receive.ReceiveTransDetails)
            {
                totalQty += rdetail.Quantity;
                if (rdetail.Quantity < 0)
                    throw new Exception("Record not saved, received quantity cannot be negative");
                if (rdetail.Quantity > po.PurchaseOrderDetails.Where(x => x.ItemNum == rdetail.ItemNum).Select(x => x.OrderQty - x.ReceiveQty).First())
                    throw new Exception("Record not saved, received quantity cannot exceed ordered qty");
            }
            if (totalQty == 0)
                throw new Exception("Record not saved, not receipt of goods found");
        }

        public void CreateReceiveTrans(ReceiveTran receive)
        {
            PurchaseOrder po = GetById(Convert.ToInt32(receive.PoNum));
            bool fulfilled = true;
            for (int i = po.PurchaseOrderDetails.Count - 1; i >= 0; i--)
            {
                int receiveQty = Convert.ToInt32(receive.ReceiveTransDetails.ElementAt(i).Quantity);
                if (receiveQty > 0)
                {
                    //update po received qty
                    po.PurchaseOrderDetails.ElementAt(i).ReceiveQty += receiveQty;
                    if (po.PurchaseOrderDetails.ElementAt(i).ReceiveQty < po.PurchaseOrderDetails.ElementAt(i).OrderQty)
                        fulfilled = false;

                    //get GST rate
                    double GST_RATE = po.GST/po.PurchaseOrderDetails.Sum(x=>x.OrderQty*x.UnitPrice);
                    //update stationery
                    Stationery s = sr.GetById(po.PurchaseOrderDetails.ElementAt(i).Stationery.ItemNum);
                    s.AverageCost = ((s.AverageCost * s.CurrentQty)
                                    + (receiveQty * po.PurchaseOrderDetails.ElementAt(i).UnitPrice) * (1 + GST_RATE))
                                    / (s.CurrentQty + receiveQty);
                    s.CurrentQty += receiveQty;
                    s.AvailableQty += receiveQty;
                    sr.Update(s);   //persist stationery data here
                }
                else if (receiveQty == 0)
                    //keep only the receive transactions details with non-zero quantity
                    receive.ReceiveTransDetails.Remove(receive.ReceiveTransDetails.ElementAt(i));
            }

            //update purchase order and create receive trans
            if (fulfilled) po.Status = "fulfilled";
            po.ReceiveTrans.Add(receive);
            Update(po);
        }

        public IEnumerable<ReceiveTransDetail> GetReceiveTransDetailByItem(string id)
        {
            return LUSSISContext.ReceiveTransDetails.Where(x => x.ItemNum == id);
        }

        public IEnumerable<PurchaseOrderDetail> GetPurchaseOrderDetailsByStatus(string status)
        {
            return LUSSISContext.PurchaseOrderDetails.Where(x => x.PurchaseOrder.Status == status);
        }


        public List<double> GetAmountByCategoryList(List<String> categoryId, String supId, String from, String to)
        {
            DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            double total = 0;

            List<double> result = new List<double>();
            foreach (String id in categoryId)
            {
                List<String> itemList = sr.GetItembyCategory(Convert.ToInt32(id));
                int supplierId = Convert.ToInt32(supId);
                total = 0;
                foreach (String item in itemList)
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

            List<double> resultList = new List<double>();
            int catId = Convert.ToInt32(category);
            foreach (String sup in supplierIds)
            {
                int supId = Convert.ToInt32(sup);

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
