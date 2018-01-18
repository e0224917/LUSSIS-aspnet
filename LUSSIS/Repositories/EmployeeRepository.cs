using LUSSIS.Models;
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

    }
}