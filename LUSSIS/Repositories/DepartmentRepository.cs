using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    public class DepartmentRepository : Repository<Department, string>
    {
        public List<String> GetAllDepartmentCode()
        {
            return LUSSISContext.Departments.Select(x => x.DeptCode).ToList();
        }

        public Department GetDepartmentByEmpNum(int empNum)
        {
            Employee emp = LUSSISContext.Employees.Where(x => x.EmpNum == empNum).FirstOrDefault();
            return emp.Department;
        }
    }
}