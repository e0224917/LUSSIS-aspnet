using System.Collections.Generic;
using System.Linq;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    //Authors: Koh Meng Guan
    public class DepartmentRepository : Repository<Department, string>
    {
        public List<string> GetAllDepartmentCode()
        {
            return LUSSISContext.Departments.Where(x=>x.DeptCode != "STNR").Select(x=>x.DeptCode).ToList();
        }

        public Department GetDepartmentByEmpNum(int empNum)
        {
            var employee = LUSSISContext.Employees.FirstOrDefault(x => x.EmpNum == empNum);
            return employee?.Department;
        }
    }
}