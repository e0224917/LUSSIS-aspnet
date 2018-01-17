using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    public class DisbursementRepository : Repository<Disbursement, string>
    {
        public Disbursement GetByDateAndDeptCode(DateTime nowDate, string deptCode)
        {
            return LUSSISContext.Disbursements.Where(x => x.CollectionDate > nowDate && x.DeptCode == deptCode).First();
        }

        public IEnumerable<DisbursementDetail> GetDisbursementDetails(Disbursement disbursement)
        {
            return LUSSISContext.DisbursementDetails.Where(x => x.DisbursementId == disbursement.DisbursementId).ToList();
        }

        public LUSSISContext LUSSISContext
        {
            get
            {
                return Context as LUSSISContext;
            }
        }
    }
}