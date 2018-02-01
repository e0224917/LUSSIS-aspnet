using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Delegate = LUSSIS.Models.Delegate;

namespace LUSSIS.Repositories
{
    //Authors: Ong Xin Ying
    public class DelegateRepository : Repository<Delegate, int>
    {
        /// <summary>
        /// Delete the delegate that exists now.
        /// </summary>
        /// <param name="deptCode"></param>
        public void DeleteByDeptCode(string deptCode)
        {
            var del = FindExistingByDeptCode(deptCode);
            Delete(del);
        }

        /// <summary>
        /// Find a delegate by employee number.
        /// </summary>
        /// <param name="empNum"></param>
        /// <returns></returns>
        public Delegate FindCurrentByEmpNum(int empNum)
        {
            return LUSSISContext.Delegates
                .FirstOrDefault(d => d.EmpNum == empNum 
                                     && d.StartDate <= DateTime.Today && d.EndDate >= DateTime.Today);
        }

        /// <summary>
        /// Find a delegate by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Delegate FindCurrentByEmail(string email)
        {
            return LUSSISContext.Delegates
                .FirstOrDefault(d => d.Employee.EmailAddress == email
                                     && d.StartDate <= DateTime.Today && d.EndDate >= DateTime.Today);
        }

        /// <summary>
        /// Find the latest delegate in a department.
        /// </summary>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public Delegate FindCurrentByDeptCode(string deptCode)
        {
            return LUSSISContext.Delegates
                .FirstOrDefault(d => d.Employee.DeptCode == deptCode
                                     && d.StartDate <= DateTime.Today && d.EndDate >= DateTime.Today);
        }

        /// <summary>
        /// Find the delegate that exist and available as of today. The delegate can be in the future.
        /// </summary>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public Delegate FindExistingByDeptCode(string deptCode)
        {
            return LUSSISContext.Delegates
                .SingleOrDefault(d => d.Employee.DeptCode == deptCode && d.EndDate >= DateTime.Today);
        }
    }
}