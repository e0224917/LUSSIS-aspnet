using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Delegate = LUSSIS.Models.Delegate;

namespace LUSSIS.Repositories
{
    public class DelegateRepository : Repository<Delegate, int>
    {
        public void DeleteByDeptCode(string deptCode)
        {
            var del = LUSSISContext.Delegates.FirstOrDefault(d => d.Employee.DeptCode == deptCode);
            Delete(del);
        }

        public Delegate GetByDeptCode(string deptCode)
        {
            return LUSSISContext.Delegates.LastOrDefault(d => d.Employee.DeptCode == deptCode);
        }

        public Delegate FindCurrentByEmpNum(int empNum)
        {
            return LUSSISContext.Delegates
                .FirstOrDefault(d => d.EmpNum == empNum 
                                     && d.StartDate <= DateTime.Today && d.EndDate >= DateTime.Today);
        }

        public Delegate FindCurrentByEmail(string email)
        {
            return LUSSISContext.Delegates
                .FirstOrDefault(d => d.Employee.EmailAddress == email
                                     && d.StartDate <= DateTime.Today && d.EndDate >= DateTime.Today);
        }

        public Delegate FindCurrentByDeptCode(string deptCode)
        {
            return LUSSISContext.Delegates
                .FirstOrDefault(d => d.Employee.DeptCode == deptCode
                                     && d.StartDate <= DateTime.Today && d.EndDate >= DateTime.Today);
        }

        public Delegate FindAllByDeptCode(string deptCode)
        {
            return LUSSISContext.Delegates
                .SingleOrDefault(d => d.Employee.DeptCode == deptCode && d.EndDate >= DateTime.Today);
        }
    }
}