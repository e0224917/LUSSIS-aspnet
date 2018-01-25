using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    public class AdjustmentDTO
    {
        public string ItemNum { get; set; }

        public int Quantity { get; set; }

        public string Reason { get; set; }

        public int RequestEmpNum { get; set; }

        public int ApprovalEmpnum { get; set; }

        public string Remark { get; set; }
    }
}