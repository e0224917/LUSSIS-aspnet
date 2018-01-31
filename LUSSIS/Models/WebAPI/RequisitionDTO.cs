using System;
using System.Collections.Generic;

namespace LUSSIS.Models.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class RequisitionDTO
    {
        public int RequisitionId { get; set; }

        public int RequisitionEmpNum { get; set; }

        public EmployeeDTO RequisitionEmp { get; set; }

        public DateTime RequisitionDate { get; set; }

        public string RequestRemarks { get; set; }

        public int ApprovalEmpNum { get; set; }

        public EmployeeDTO ApprovalEmp { get; set; }

        public string ApprovalRemarks { get; set; }

        public string Status { get; set; }

        public IEnumerable<RequisitionDetailDTO> RequisitionDetails { get; set; }
    }
}