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

        public Department GetDepartmentByUser(Employee employee)
        {
            return LUSSISContext.Departments.First(y => y.DeptCode == employee.DeptCode);
        }

        public Department GetDepartmentByEmpNum(int empNum)
        {
            Employee emp = GetById(empNum);
            return emp.Department;
        }

        public List<Employee> GetAllByDepartment(Department department)
        {
            return LUSSISContext.Employees.Where(k => k.DeptCode == department.DeptCode).ToList();
        }
        
        public List <LUSSIS.Models.Delegate> GetAllDelegates()
        {
            return LUSSISContext.Delegates.ToList();
        }

        public Models.Delegate GetDelegateByDate(Department department, DateTime dateTime)
        {
            List<Employee> empList = GetAllByDepartment(department);
            List<Models.Delegate> delList = GetAllDelegates();
            List<Models.Delegate> allDel = delList.Where(x => empList.Any(y => y.EmpNum == x.EmpNum)).ToList();
            return allDel.FirstOrDefault(k => k.StartDate <= dateTime && k.EndDate >= dateTime);
        }

        public bool CheckIfLoggedInUserIsDelegate()
        {
            int employeeNum = GetCurrentUser().EmpNum;
            DateTime dateTime = DateTime.Today.Date;
            Models.Delegate meDelegate = GetAllDelegates().FirstOrDefault(x => x.EmpNum == employeeNum && x.StartDate <= dateTime && x.EndDate >= dateTime);
            if (meDelegate == null)
            {
                return false;
            }
            else { return true; }
        }

        public bool CheckIfUserDepartmentHasDelegate()
        {
            Department meDept = GetCurrentUser().Department;
            Models.Delegate meDeptDelegate = GetDelegateByDate(meDept, DateTime.Today.Date);
            if (meDeptDelegate == null)
            {
                return false;
            }
            else { return true; }
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