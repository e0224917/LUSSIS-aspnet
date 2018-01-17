using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;
using LUSSIS.Repositories.Interface;

namespace LUSSIS.Repositories
{
    public class DisbursementRepository : Repository<Disbursement, int>, IDisbursementRepository
    {
        public List<DisbursementDetail> GetDisbursementDetailsByStatus(string status)
        {
            return Context.DisbursementDetails.Where(d => d.Disbursement.Status == status).ToList();
        }
        
    }
}