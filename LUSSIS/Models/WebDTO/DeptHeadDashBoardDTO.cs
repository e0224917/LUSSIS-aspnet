using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class DeptHeadDashBoardDTO
    {
        public int GetRequisitionListCount { get; set; }

        public Delegate GetDelegate { get; set; }

        public Employee GetRep { get; set; }

        public Department Department { get; set; }

        public List<Employee> GetStaffRepByDepartment { get; set; }

        public Delegate GetDelegateByDate { get; set; }
    }
}