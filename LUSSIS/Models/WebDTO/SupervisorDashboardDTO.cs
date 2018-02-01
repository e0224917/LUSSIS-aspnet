using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: May Zin Ko
    public class SupervisorDashboardDTO
    {
        public double PendingPOTotalAmount { get; set; }

        public int PendingPOCount { get; set; }

        public double POTotalAmount { get; set; }

        public int PendingStockAdjAddQty { get; set; }

        public int PendingStockAdjSubtractQty { get; set; }

        public int PendingStockAdjCount { get; set; }

        public double TotalDisbursementAmount { get; set; }

        public List<String> CharterName { get; set; }

        public List<double> CharterValue { get; set; }

        public List<String> PieName { get; set; }

        public List<double> PieValue { get; set; }

        public int GetPendingAdjustmentByRole { get; set; }

    }
}