using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class ReqApproveRejectDTO
    {

        public string ApprovalRemarks { get; set; }
        public int RequisitionId { get; set; }
        public string Status { get; set; }

    }
}