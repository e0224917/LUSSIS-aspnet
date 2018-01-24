using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Repositories;

namespace LUSSIS.Models.WebDTO
{
    public class DelegateStaffDTO
    {
        EmployeeRepository empRepo = new EmployeeRepository();
        public bool IsThisADelegate()
        {
             return empRepo.CheckIfLoggedInUserIsDelegate();
       
        }
    }
}