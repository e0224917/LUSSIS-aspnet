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
        DisbursementRepository disRepo = new DisbursementRepository();

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

        public List<Employee> GetAllByDepartment(Department department)
        {
            return LUSSISContext.Employees.Where(k => k.DeptCode == department.DeptCode).ToList();
        }

        public List<Employee> GetStaffRepByDepartment(Department department)
        {
            return LUSSISContext.Employees.Where(z => z.DeptCode == department.DeptCode
            && (z.JobTitle == "rep" || z.JobTitle == "staff")).ToList();
        }

        public List<Employee> GetStaffOnlyByDepartment(Department department)
        {
            return LUSSISContext.Employees.Where(y => y.DeptCode == department.DeptCode 
            && y.JobTitle == "staff").ToList();
        }

        public List<Employee> GetStaffByDepartmentCode(String deptCode)
        {
            return LUSSISContext.Employees.Where(y => y.DeptCode == deptCode
                                                      && y.JobTitle == "staff").ToList();
        }

        public List<Employee> GetAllStoreClerk()
        {
            return GetStaffByDepartmentCode("STNR").Where(x => x.JobTitle == "clerk").ToList();
        }

        public List<Employee> GetSelectionByDepartment(string prefix, Department department)
        {
            List<Employee> employee = GetStaffRepByDepartment(department);
            return employee.Where(x => x.FullName.Contains(prefix)).ToList();
        }


        public List<Employee> GetDelSelectionByDepartment(string prefix, Department department)
        {
            List<Employee> employee = GetStaffOnlyByDepartment(department);
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
            UpdateDepartment(department);
        }
        
        public List <LUSSIS.Models.Delegate> GetAllDelegates()
        {
            return LUSSISContext.Delegates.ToList();
        }

        public Models.Delegate GetFutureDelegate(Department department, DateTime dateTime)
        {
            List<Employee> empList = GetAllByDepartment(department);
            List<Models.Delegate> delList = GetAllDelegates();
            List <Models.Delegate> allDel = delList.Where(x => empList.Any(y => y.EmpNum == x.EmpNum)).ToList();
            return allDel.Where(y => y.EndDate >= dateTime).FirstOrDefault();
        }

        public Models.Delegate GetDelegateByDate(Department department, DateTime dateTime)
        {
            List<Employee> empList = GetAllByDepartment(department);
            List<Models.Delegate> delList = GetAllDelegates();
            List<Models.Delegate> allDel = delList.Where(x => empList.Any(y => y.EmpNum == x.EmpNum)).ToList();
            return allDel.Where(k => k.StartDate <= dateTime && k.EndDate >= dateTime).FirstOrDefault();
        }

        public bool CheckIfLoggedInUserIsDelegate()
        {
            int employeeNum = GetCurrentUser().EmpNum;
            DateTime dateTime = DateTime.Today.Date;
            Models.Delegate meDelegate = GetAllDelegates().Where(x => x.EmpNum == employeeNum && x.StartDate <= dateTime && x.EndDate >= dateTime).FirstOrDefault();
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
    

        public void DeleteDelegate(Department department)
        {
            Models.Delegate del = GetFutureDelegate(department, DateTime.Now.Date);
            LUSSISContext.Delegates.Remove(del);
            LUSSISContext.SaveChanges();
        }
       
        public List<Department> GetDepartmentAll()
        {
            List<Department> depList = LUSSISContext.Departments.ToList();

            return depList;

        }

        
        public List<String> GetDepartmentNames()
        {
            return LUSSISContext.Departments.Select(x => x.DeptName).ToList();
        }
        public List<double> GetDepartmentValue()
        {
            List<Department> depList = GetDepartmentAll();
            List<double> valueList = new List<double>();
            foreach (Department e in depList)
            {
               
                valueList.Add(disRepo.GetDisbursementByDepCode(e.DeptCode));
                
            }
            return valueList;
        }
    }
}