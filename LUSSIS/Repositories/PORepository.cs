using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LUSSIS.Models;
using LUSSIS.Repositories.Interface;

namespace LUSSIS.Repositories
{
    public class PORepository : Repository<PurchaseOrder, int>, IPORepository
    {
        public PORepository(){ }

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

    }
}