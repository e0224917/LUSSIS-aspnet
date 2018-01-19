using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    public class RequisitionDTO
    {
        public int RequisitionId { get; set; }

        public string RequisitionEmp { get; set; }

        public DateTime? RequisitionDate { get; set; }

        public string RequestRemarks { get; set; }

        public string ApprovalEmp { get; set; }

        public string ApprovalRemarks { get; set; }

        public string Status { get; set; }

        public IEnumerable<RequisitionDetailDTO> RequisitionDetails { get; set; }
    }
}