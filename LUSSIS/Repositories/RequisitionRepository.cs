using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    public class RequisitionRepository: Repository<Requisition, int>
    {
        public List<Requisition> GetPendingRequisitions()
        {
            IEnumerable<Requisition> list = GetAll().Where(x => x.Status == "approved");
            return list.ToList();
        }



    }
}