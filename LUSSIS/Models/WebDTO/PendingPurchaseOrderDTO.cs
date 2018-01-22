using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class PendingPurchaseOrderDTO
    {
     
            public int PoNum { get; set; }

            public String Supplier { get; set; }


            public DateTime? CreateDate { get; set; }


            public String OrderEmp { get; set; }

            public double TotalCost { get; set; }
        }
    
}