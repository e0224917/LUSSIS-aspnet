using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class SupervisorDashboardDTO
    {
        public double PendingPOTotalAmount { get; set; }

        public int PendingPOCount { get; set; }

        public double POTotalAmount { get; set; }

        public int PendingStockAdjAddQty { get; set; }

        public int PendingStockAdjSubtractQty { get; set; }

        public int PendingStockAdjCount { get; set; }


    }
}