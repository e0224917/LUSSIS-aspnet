using LUSSIS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    public class EmployeeRepository : Repository<Employee, string>
    {
        public Employee GetEmployeeByEmail(string email)
        {
            return LUSSISContext.Employees.First(x => x.EmailAddress == email);
        }

        public Department GetDepartmentByUser(Employee employee)
        {
            return LUSSISContext.Departments.First(y => y.DeptCode == employee.DeptCode);
        }

        public void UpdateDepartment(Department department)
        {
            LUSSISContext.SaveChanges();
        }
        public Employee GetCurrentUser() {
            string userName = System.Web.HttpContext.Current.User.Identity.GetUserName();
            return GetEmployeeByEmail(userName);
        }
        

    }
}