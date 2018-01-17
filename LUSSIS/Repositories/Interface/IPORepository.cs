using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Repositories.Interface
{
    interface IPORepository : IRepository<PurchaseOrder, int>
    {
        List<PurchaseOrder> GetPendingApprovalPO();
        List<PurchaseOrder> GetApprovedPO();
        List<PurchaseOrder> GetPOByStatus(string status);

    }
}