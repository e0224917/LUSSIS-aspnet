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

        public LUSSISContext LUSSISContext
        {
            get { return Context as LUSSISContext; }
        }

        public List<PurchaseOrder> GetPendingApprovalPO()
        {
            IEnumerable<PurchaseOrder> list = GetAll().Where(x => x.Status == "fulfilled");
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
        public double GetPendingPOTotalAmount()
        {
            double result = 0;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            list = GetPendingApprovalPO();

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
        public SupervisorDashboardDTO GetSupervisorDashboard()
        {
            SupervisorDashboardDTO dash = new SupervisorDashboardDTO();

            dash.PendingPOTotalAmount = GetPendingPOTotalAmount();
            dash.PendingPOCount = GetPendingPOCount();
            dash.POTotalAmount = GetPOTotalAmount();
            dash.PendingStockAdjAddQty = 0;
            dash.PendingStockAdjSubtractQty = 0;
            dash.PendingStockAdjCount = 0;

            return dash;

        }

        public double GetPOTotalAmount()
        {
            double result = 0;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            list = GetAll().ToList<PurchaseOrder>();

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

        public int GetPendingPOCount()
        {
            List<PurchaseOrder> list = GetPendingApprovalPO();
            return list.Capacity;
        }

    }
}