using LUSSIS.Constants;
using LUSSIS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    //Authors: Ong Xin Ying, Tang Xiaowen
    public class EmployeeRepository : Repository<Employee, int>
    {
        public Employee GetEmployeeByEmail(string email)
        {
            return LUSSISContext.Employees.First(x => x.EmailAddress == email);
        }

        /// <summary>
        /// Return list of employees in a department, except the department head.
        /// This list is used to choose for delegate.
        /// </summary>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public List<Employee> GetStaffRepByDeptCode(string deptCode)
        {
            return LUSSISContext.Employees.Where(x => x.DeptCode == deptCode && (x.JobTitle == Role.Staff || x.JobTitle == Role.Representative)).ToList();
        }

        /// <summary>
        /// Return list of employees to assign a new rep.
        /// </summary>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public List<Employee> GetStaffByDeptCode(string deptCode)
        {
            return LUSSISContext.Employees.Where(x => x.DeptCode == deptCode && x.JobTitle == Role.Staff).ToList();
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