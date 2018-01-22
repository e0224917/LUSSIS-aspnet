using System;
using System.Collections.Generic;
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
            List<PurchaseOrderDetail> pd_list = LUSSISContext.PurchaseOrderDetails.Where(x => x.PoNum.Equals(poNum)).ToList<PurchaseOrderDetail>();
            double total = 0;
            foreach (PurchaseOrderDetail pod in pd_list)
            {
                //int qty = from t in LUSSISContext.PurchaseOrderDetails select t.OrderQty;

                int qty = (int)LUSSISContext.PurchaseOrderDetails.Select(x => x.OrderQty).ToList()[0];
                double unit_price = (double)LUSSISContext.PurchaseOrderDetails.Select(x => x.UnitPrice).ToList()[0];
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
                List<PurchaseOrderDetail> pd_list = LUSSISContext.PurchaseOrderDetails.Where(x => x.PoNum.Equals(po.PoNum)).ToList<PurchaseOrderDetail>();

                foreach (PurchaseOrderDetail pod in pd_list)
                {
                    //int qty = from t in LUSSISContext.PurchaseOrderDetails select t.OrderQty;
                    int qty = (int)LUSSISContext.PurchaseOrderDetails.Select(x => x.OrderQty).ToList()[0];
                    double unit_price = (double)LUSSISContext.PurchaseOrderDetails.Select(x => x.UnitPrice).ToList()[0];
                    result += (qty * unit_price);

                }

            }
            return result;
        }

      
        public List<PurchaseOrderDetail> GetPODetailsByPoNum(int poNum)
        {
            return LUSSISContext.PurchaseOrderDetails.Where(x => x.PoNum == poNum).ToList();
        }
        public void UpDatePOStatus(int i, String status)
        {
            PurchaseOrder p = GetById(i);
            p.Status = status;
            p.ApprovalDate = DateTime.Today;
            Update(p);
        }


    }
}