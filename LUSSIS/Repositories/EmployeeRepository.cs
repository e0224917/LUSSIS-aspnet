using LUSSIS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Exceptions;

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

        public List<Employee> GetStaffRepByDepartment(Department department)
        {
            return LUSSISContext.Employees.Where(z => z.DeptCode == department.DeptCode 
            && (z.JobTitle == "rep" || z.JobTitle == "staff")).ToList();
        }

        public List<Employee> GetSelectionByDepartment(string prefix, Department department)
        {
            List<Employee> employee = GetStaffRepByDepartment(department);
            return employee.Where(x => x.FullName.Contains(prefix)).ToList();
        }

        public void UpdateDepartment(Department department)
        {
            LUSSISContext.SaveChanges();
        }

        public void ChangeRep(Department department, string repEmp)
        {
            //int repEmpInt;
            //bool result = Int32.TryParse(repEmp, out repEmpInt);
            //if(!result)
            //{
            //    throw new InvalidSetRepException("No Employee found");
            //}
            department.RepEmployee.JobTitle = "staff";
            Update(department.RepEmployee);
            department.RepEmpNum = Convert.ToInt32(repEmp);
            UpdateDepartment(department);
            department.RepEmployee.JobTitle = "rep";
            Update(department.RepEmployee);
        }

        public Employee GetCurrentUser() {
            string userName = System.Web.HttpContext.Current.User.Identity.GetUserName();
            return GetEmployeeByEmail(userName);
        }
        

    }
}