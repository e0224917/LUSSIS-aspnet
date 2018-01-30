using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Ong Xin Ying
    public class RepAndDelegateDTO
    {
        public Department Department { get; set; }

        public List<Employee> StaffAndRepList { get; set; }

        public Delegate MyDelegate { get; set; }

        public Employee DeptRep { get; set; }
    }
}