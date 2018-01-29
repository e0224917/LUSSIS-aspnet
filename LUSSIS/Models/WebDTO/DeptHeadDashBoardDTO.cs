using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Ong Xin Ying
    public class DeptHeadDashBoardDTO
    {
        public int RequisitionListCount { get; set; }

        public Delegate CurrentDelegate { get; set; }

        public Employee CurrentRep { get; set; }

        public Department Department { get; set; }

        public IEnumerable<Employee> StaffRepByDepartment { get; set; }

        public bool HaveDelegateToday { get; set; }
    }
}