using LUSSIS.Constants;
using LUSSIS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    //Authors: Ong Xin Ying
    public class EmployeeRepository : Repository<Employee, int>
    {
        public Employee GetEmployeeByEmail(string email)
        {
            return LUSSISContext.Employees.First(x => x.EmailAddress == email);
        }

        public Employee GetStoreManager()
        {
            return LUSSISContext.Employees.FirstOrDefault(x => x.JobTitle == Role.Manager);
        }

        public Employee GetStoreSupervisor()
        {
            return LUSSISContext.Employees.FirstOrDefault(x => x.JobTitle == Role.Supervisor);
        }

        public Employee GetDepartmentHead(string deptCode)
        {
            return LUSSISContext.Employees.SingleOrDefault(e => e.DeptCode == deptCode && e.JobTitle == Role.DepartmentHead);
        }

        public Employee GetRepByDeptCode(string deptCode)
        {
            return LUSSISContext.Employees.SingleOrDefault(e => e.DeptCode == deptCode && e.JobTitle == Role.Representative);
        }
    }
}