using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    public class DelegateRepository : Repository<Models.Delegate, int>
    {
        public void DeleteByDeptCode(string deptCode)
        {
            var del = LUSSISContext.Delegates.FirstOrDefault(d => d.Employee.DeptCode == deptCode);
            Delete(del);
        }

        public Delegate GetByDeptCode(string deptCode)
        {
            return LUSSISContext.Delegates.FirstOrDefault(d => d.Employee.DeptCode == deptCode);
        }
    }
}