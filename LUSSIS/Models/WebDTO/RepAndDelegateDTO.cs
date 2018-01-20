using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class RepAndDelegateDTO
    {
        public Department Department { get; set; }

        public List<Employee> GetStaffRepByDepartment { get; set; }
    }
}