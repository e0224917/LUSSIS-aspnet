using LUSSIS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Exceptions;
using LUSSIS.Extensions;

namespace LUSSIS.Repositories
{
    public class EmployeeRepository : Repository<Employee, int>
    {

        public Employee GetCurrentUser()
        {
            string userName = System.Web.HttpContext.Current.User.Identity.GetUserName();
            return GetEmployeeByEmail(userName);
        }

        public string GetJobTitleByEmail(string email)
        {
            return GetEmployeeByEmail(email).JobTitle;
        }

        public Employee GetEmployeeByEmail(string email)
        {
            return LUSSISContext.Employees.First(x => x.EmailAddress == email);
        }

        public Department GetDepartmentByEmpNum(int empNum)
        {
            return GetById(empNum).Department;
        }

        public Employee GetStoreManager()
        {
            return LUSSISContext.Employees.FirstOrDefault(x => x.JobTitle == "manager");
        }
        public Employee GetStoreSupervisor()
        {
            return LUSSISContext.Employees.FirstOrDefault(x => x.JobTitle == "supervisor");
        }
        public List<String>GetAllDepartmentCode()
        {
            return LUSSISContext.Departments.Select(x => x.DeptCode).ToList();
        }
        public List<Department>GetAllDepartment()
        {
            return LUSSISContext.Departments.ToList();
        }
      
    }
}